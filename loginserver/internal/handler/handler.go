package handler

import (
	"encoding/json"
	"net/http"
	"strings"

	"loginserver/internal/storage"
)

type VerifyResponse struct {
	Code     int    `json:"code"`
	UUID     string `json:"uuid,omitempty"`
	Username string `json:"username,omitempty"`
	Message  string `json:"message,omitempty"`
}

func HandleVerify(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, VerifyResponse{
				Code: 1004, Message: "Method not allowed",
			})
			return
		}

		authHeader := r.Header.Get("Authorization")
		if authHeader == "" {
			writeJSON(w, http.StatusUnauthorized, VerifyResponse{
				Code: 1004, Message: "Authorization header required",
			})
			return
		}

		token := strings.TrimPrefix(authHeader, "Bearer ")
		if token == authHeader {
			writeJSON(w, http.StatusUnauthorized, VerifyResponse{
				Code: 1004, Message: "Bearer token required",
			})
			return
		}

		userID := parseToken(token)
		if userID == "" {
			writeJSON(w, http.StatusUnauthorized, VerifyResponse{
				Code: 1002, Message: "Invalid token",
			})
			return
		}

		user, exists := store.GetUserByID(userID)
		if !exists {
			writeJSON(w, http.StatusUnauthorized, VerifyResponse{
				Code: 1003, Message: "User not found",
			})
			return
		}

		writeJSON(w, http.StatusOK, VerifyResponse{
			Code:     0,
			UUID:     user.ID,
			Username: user.Username,
		})
	}
}

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}
