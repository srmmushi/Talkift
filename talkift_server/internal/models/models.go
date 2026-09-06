package models

import "time"

type User struct {
	ID                 string    `json:"id"`
	Username           string    `json:"username"`
	PasswordHash       string    `json:"password_hash"`
	RegisterMethod     string    `json:"register_method"`
	RegisterServerIP   string    `json:"register_server_ip,omitempty"`
	CreatedAt          time.Time `json:"created_at"`
	LastLoginAt        time.Time `json:"last_login_at"`
	MutedConversations []string  `json:"muted_conversations,omitempty"`
}

type Message struct {
	ID             string    `json:"id"`
	ConversationID string    `json:"conversation_id"`
	SenderID       string    `json:"sender_id"`
	SenderName     string    `json:"sender_name,omitempty"`
	Content        string    `json:"content"`
	Type           string    `json:"type"`
	Timestamp      time.Time `json:"timestamp"`
}

type ConversationType string

const (
	ConversationTypeGroup        ConversationType = "group"
	ConversationTypeConversation ConversationType = "conversation"
)

type Conversation struct {
	ID            string           `json:"id"`
	Name          string           `json:"name"`
	Type          ConversationType `json:"type"`
	OwnerID       string           `json:"owner_id"`
	Members       []string         `json:"members"`
	CreatedAt     time.Time        `json:"created_at"`
	IsPublic      bool             `json:"is_public"`
	MutedMembers  []string         `json:"muted_members,omitempty"`
	BannedMembers []string         `json:"banned_members,omitempty"`
}

type WSMessage struct {
	Type    string      `json:"type"`
	Payload interface{} `json:"payload"`
}
