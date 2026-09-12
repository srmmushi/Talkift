package model

import "time"

type User struct {
	ID             string    `json:"id"`
	Username       string    `json:"username"`
	Email          string    `json:"email,omitempty"`
	PasswordHash   string    `json:"password_hash"`
	RegisterMethod string    `json:"register_method"`
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
}
