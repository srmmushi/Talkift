package handler

import (
	"encoding/json"
	"net/http"
	"strings"

	"official-server/internal/storage"
)

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}

func decodeBody(r *http.Request, v interface{}) error {
	return json.NewDecoder(r.Body).Decode(v)
}

func extractUserID(r *http.Request) string {
	authHeader := r.Header.Get("Authorization")
	if authHeader == "" {
		return ""
	}
	token := strings.TrimPrefix(authHeader, "Bearer ")
	if token == authHeader {
		return ""
	}
	parts := strings.SplitN(token, "|", 2)
	if len(parts) < 1 {
		return ""
	}
	return parts[0]
}

func HandleVerify(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		userID := extractUserID(r)
		if userID == "" {
			writeJSON(w, http.StatusUnauthorized, map[string]interface{}{"code": 1002, "message": "Unauthorized"})
			return
		}

		user, exists := store.GetUserByID(userID)
		if !exists {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "User not found"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":     0,
			"uuid":     user.ID,
			"username": user.Username,
			"email":    user.Email,
		})
	}
}

func HandleLogout(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		userID := extractUserID(r)
		if userID != "" {
			store.SetOnlineStatus(userID, false)
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Logged out"})
	}
}
