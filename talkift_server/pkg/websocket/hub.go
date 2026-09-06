package websocket

import (
	"encoding/json"
	"net/http"
	"sync"
	"time"

	"talkift_server/internal/utils"

	"github.com/gorilla/websocket"
)

const (
	writeWait      = 10 * time.Second
	pongWait       = 60 * time.Second
	pingPeriod     = (pongWait * 9) / 10
	maxMessageSize = 8192
)

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
}

type Client struct {
	ID     string
	UserID string
	hub    *Hub
	conn   *websocket.Conn
	send   chan []byte
	once   sync.Once
}

type Hub struct {
	clients    map[*Client]bool
	userClients map[string][]*Client
	broadcast  chan []byte
	register   chan *Client
	unregister chan *Client
	mu         sync.RWMutex
}

func NewHub() *Hub {
	return &Hub{
		clients:     make(map[*Client]bool),
		userClients: make(map[string][]*Client),
		broadcast:   make(chan []byte, 256),
		register:    make(chan *Client),
		unregister:  make(chan *Client),
	}
}

func (h *Hub) Run() {
	for {
		select {
		case client := <-h.register:
			h.mu.Lock()
			h.clients[client] = true
			if client.UserID != "" {
				h.userClients[client.UserID] = append(h.userClients[client.UserID], client)
			}
			h.mu.Unlock()
			utils.Infof("client connected: %s (user=%s)", client.ID, client.UserID)

		case client := <-h.unregister:
			h.mu.Lock()
			if _, ok := h.clients[client]; ok {
				delete(h.clients, client)
				if client.UserID != "" {
					clients := h.userClients[client.UserID]
					for i, c := range clients {
						if c == client {
							h.userClients[client.UserID] = append(clients[:i], clients[i+1:]...)
							break
						}
					}
					if len(h.userClients[client.UserID]) == 0 {
						delete(h.userClients, client.UserID)
					}
				}
				client.close()
			}
			h.mu.Unlock()
			utils.Infof("client disconnected: %s (user=%s)", client.ID, client.UserID)

		case message := <-h.broadcast:
			h.mu.RLock()
			for client := range h.clients {
				select {
				case client.send <- message:
				default:
					client.close()
					delete(h.clients, client)
				}
			}
			h.mu.RUnlock()
		}
	}
}

func (h *Hub) BroadcastMessage(message []byte) {
	h.broadcast <- message
}

func (h *Hub) BroadcastJSON(msg interface{}) error {
	data, err := json.Marshal(msg)
	if err != nil {
		return err
	}
	h.broadcast <- data
	return nil
}

func (h *Hub) SendToUserID(userID string, msg interface{}) error {
	data, err := json.Marshal(msg)
	if err != nil {
		return err
	}
	return h.SendBytesToUserID(userID, data)
}

func (h *Hub) SendBytesToUserID(userID string, data []byte) error {
	h.mu.RLock()
	defer h.mu.RUnlock()

	clients := h.userClients[userID]
	for _, c := range clients {
		select {
		case c.send <- data:
		default:
		}
	}
	return nil
}

func (h *Hub) SendToUserIDs(userIDs []string, msg interface{}) {
	data, err := json.Marshal(msg)
	if err != nil {
		return
	}

	h.mu.RLock()
	defer h.mu.RUnlock()

	for _, uid := range userIDs {
		clients := h.userClients[uid]
		for _, c := range clients {
			select {
			case c.send <- data:
			default:
			}
		}
	}
}

func (h *Hub) SendToClient(clientID string, message []byte) bool {
	h.mu.RLock()
	defer h.mu.RUnlock()

	for c := range h.clients {
		if c.ID == clientID {
			select {
			case c.send <- message:
				return true
			default:
				return false
			}
		}
	}
	return false
}

func (h *Hub) SetClientUserID(clientID, userID string) {
	h.mu.Lock()
	defer h.mu.Unlock()

	for c := range h.clients {
		if c.ID == clientID {
			if c.UserID != "" {
				clients := h.userClients[c.UserID]
				for i, cl := range clients {
					if cl == c {
						h.userClients[c.UserID] = append(clients[:i], clients[i+1:]...)
						break
					}
				}
				if len(h.userClients[c.UserID]) == 0 {
					delete(h.userClients, c.UserID)
				}
			}
			c.UserID = userID
			h.userClients[userID] = append(h.userClients[userID], c)
			break
		}
	}
}

func (c *Client) close() {
	c.once.Do(func() {
		close(c.send)
	})
}

func ServeWs(hub *Hub, w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		utils.Errorf("websocket upgrade failed: %v", err)
		return
	}

	client := &Client{
		ID:   utils.NewUUID(),
		hub:  hub,
		conn: conn,
		send: make(chan []byte, 256),
	}

	hub.register <- client

	go client.writePump()
	go client.readPump()
}

func (c *Client) readPump() {
	defer func() {
		c.hub.unregister <- c
		c.conn.Close()
	}()

	c.conn.SetReadLimit(maxMessageSize)
	c.conn.SetReadDeadline(time.Now().Add(pongWait))
	c.conn.SetPongHandler(func(string) error {
		c.conn.SetReadDeadline(time.Now().Add(pongWait))
		return nil
	})

	for {
		_, message, err := c.conn.ReadMessage()
		if err != nil {
			if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseNormalClosure) {
				utils.Errorf("websocket read error: %v", err)
			}
			break
		}
		c.hub.broadcast <- message
	}
}

func (c *Client) writePump() {
	ticker := time.NewTicker(pingPeriod)
	defer func() {
		ticker.Stop()
		c.conn.Close()
	}()

	for {
		select {
		case message, ok := <-c.send:
			c.conn.SetWriteDeadline(time.Now().Add(writeWait))
			if !ok {
				c.conn.WriteMessage(websocket.CloseMessage, []byte{})
				return
			}

			w, err := c.conn.NextWriter(websocket.TextMessage)
			if err != nil {
				return
			}
			w.Write(message)

			n := len(c.send)
			for i := 0; i < n; i++ {
				w.Write([]byte{'\n'})
				w.Write(<-c.send)
			}

			if err := w.Close(); err != nil {
				return
			}

		case <-ticker.C:
			c.conn.SetWriteDeadline(time.Now().Add(writeWait))
			if err := c.conn.WriteMessage(websocket.PingMessage, nil); err != nil {
				return
			}
		}
	}
}
