package model

import "time"

type User struct {
	ID             string    `json:"id"`
	Username       string    `json:"username"`
	Email          string    `json:"email,omitempty"`
	PasswordHash   string    `json:"password_hash"`
	RegisterMethod string    `json:"register_method"`
	Avatar         string    `json:"avatar,omitempty"`
	Bio            string    `json:"bio,omitempty"`
	Status         string    `json:"status,omitempty"`
	IsOnline       bool      `json:"is_online"`
	IsBanned       bool      `json:"is_banned"`
	BanReason      string    `json:"ban_reason,omitempty"`
	CreatedAt      time.Time `json:"created_at"`
	LastLoginAt    time.Time `json:"last_login_at"`
}

type VersionInfo struct {
	Version      string `json:"version"`
	MinClient    string `json:"min_client"`
	UpdateUrl    string `json:"update_url"`
	ReleaseNotes string `json:"release_notes"`
	ReleaseDate  string `json:"release_date"`
	IsLatest     bool   `json:"is_latest"`
	FileSize     int64  `json:"file_size"`
	FileHash     string `json:"file_hash"`
}

type ServerConfig struct {
	ChatHost     string `json:"chat_host"`
	ChatPort     int    `json:"chat_port"`
	ChatProtocol string `json:"chat_protocol"`
}

type BanEntry struct {
	UserID    string    `json:"user_id"`
	Reason    string    `json:"reason"`
	BannedBy  string    `json:"banned_by"`
	BannedAt  time.Time `json:"banned_at"`
	ExpiresAt time.Time `json:"expires_at,omitempty"`
}

type AuditLog struct {
	ID        string    `json:"id"`
	Action    string    `json:"action"`
	UserID    string    `json:"user_id"`
	Username  string    `json:"username"`
	Details   string    `json:"details,omitempty"`
	IP        string    `json:"ip"`
	CreatedAt time.Time `json:"created_at"`
}

type APIKey struct {
	ID        string    `json:"id"`
	Name      string    `json:"name"`
	Key       string    `json:"key"`
	UserID    string    `json:"user_id"`
	Scopes    []string  `json:"scopes"`
	ExpiresAt time.Time `json:"expires_at,omitempty"`
	CreatedAt time.Time `json:"created_at"`
	LastUsed  time.Time `json:"last_used,omitempty"`
}

type Notification struct {
	ID        string    `json:"id"`
	UserID    string    `json:"user_id"`
	Title     string    `json:"title"`
	Body      string    `json:"body"`
	Type      string    `json:"type"`
	Read      bool      `json:"read"`
	CreatedAt time.Time `json:"created_at"`
}

type FriendRequest struct {
	ID        string    `json:"id"`
	FromUser  string    `json:"from_user"`
	ToUser    string    `json:"to_user"`
	Status    string    `json:"status"`
	Message   string    `json:"message,omitempty"`
	CreatedAt time.Time `json:"created_at"`
}

type BlockEntry struct {
	BlockerID  string    `json:"blocker_id"`
	BlockedID  string    `json:"blocked_id"`
	CreatedAt  time.Time `json:"created_at"`
}
