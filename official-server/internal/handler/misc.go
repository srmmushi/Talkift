package handler

import (
	"net/http"

	"official-server/internal/storage"
)

func HandleLogout() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		userID := extractUserID(r)
		if userID != "" {
			storage.SetOnlineStatusGlobal(userID, false)
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Logged out"})
	}
}

func HandleSetAvatar(store *storage.Storage) http.HandlerFunc {
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

		var req struct {
			Avatar string `json:"avatar"`
		}
		if err := decodeJSON(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.UpdateUser(userID, map[string]interface{}{"avatar": req.Avatar}); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to update avatar"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Avatar updated"})
	}
}

func HandleBlockUser() http.HandlerFunc {
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

		var req struct {
			BlockedUserID string `json:"blocked_user_id"`
		}
		if err := decodeJSON(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "User blocked"})
	}
}

func HandleGetConfig(cfg interface{}) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":    0,
			"message": "Config retrieved",
		})
	}
}

func HandleGetChatConfig(versionStore interface{}) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":    0,
			"message": "Chat config retrieved",
		})
	}
}

func decodeJSON(r *http.Request, v interface{}) error {
	return decodeJSONBody(r, v)
}
