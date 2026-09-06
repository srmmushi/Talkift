package handlers

import (
	"encoding/json"
	"net/http"

	"talkift_server/internal/config"
	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
	"talkift_server/internal/storage"
	"talkift_server/internal/utils"
	"talkift_server/pkg/websocket"
)

type Handlers struct {
	store storage.Storage
	hub   *websocket.Hub
	cfg   *config.Config
}

func New(store storage.Storage, hub *websocket.Hub, cfg *config.Config) *Handlers {
	return &Handlers{store: store, hub: hub, cfg: cfg}
}

func (h *Handlers) HandleWebSocket(w http.ResponseWriter, r *http.Request) {
	websocket.ServeWs(h.hub, w, r)
}

func (h *Handlers) HandleConversations(w http.ResponseWriter, r *http.Request) {
	userID := middleware.GetUserID(r)
	w.Header().Set("Content-Type", "application/json")

	switch r.Method {
	case http.MethodGet:
		convs, err := h.store.ListConversations(userID)
		if err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}
		json.NewEncoder(w).Encode(convs)

	case http.MethodPost:
		var conv models.Conversation
		if err := json.NewDecoder(r.Body).Decode(&conv); err != nil {
			h.writeError(w, models.CodeInvalidRequest, http.StatusBadRequest)
			return
		}
		conv.ID = utils.NewUUID()
		conv.OwnerID = userID
		if conv.Members == nil {
			conv.Members = []string{userID}
		}

		if err := h.store.SaveConversation(&conv); err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(conv)

	default:
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
	}
}

func (h *Handlers) HandleMessages(w http.ResponseWriter, r *http.Request) {
	convID := r.URL.Query().Get("conversation_id")
	if convID == "" {
		h.writeJSON(w, http.StatusBadRequest, models.ErrorPayload{
			Code:    models.CodeInvalidRequest,
			Message: "conversation_id is required",
		})
		return
	}
	w.Header().Set("Content-Type", "application/json")

	switch r.Method {
	case http.MethodGet:
		msgs, err := h.store.LoadMessages(convID, 50, 0)
		if err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}
		json.NewEncoder(w).Encode(msgs)

	case http.MethodPost:
		userID := middleware.GetUserID(r)
		var msg models.Message
		if err := json.NewDecoder(r.Body).Decode(&msg); err != nil {
			h.writeError(w, models.CodeInvalidRequest, http.StatusBadRequest)
			return
		}
		msg.ID = utils.NewUUID()
		msg.ConversationID = convID
		msg.SenderID = userID

		if err := h.store.SaveMessage(&msg); err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}

		h.hub.BroadcastJSON(models.WSMessage{
			Type:    string(models.MsgTypeChat),
			Payload: msg,
		})

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(msg)

	default:
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
	}
}

func (h *Handlers) writeJSON(w http.ResponseWriter, statusCode int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(data)
}

func (h *Handlers) writeError(w http.ResponseWriter, code int, httpStatus int) {
	h.writeJSON(w, httpStatus, models.ErrorPayload{
		Code:    code,
		Message: models.ErrorMessage(code),
	})
}
