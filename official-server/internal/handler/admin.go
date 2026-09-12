package handler

import (
	"net/http"

	"official-server/internal/config"
	"official-server/internal/storage"
)

func HandleGetUsers(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		users := store.GetAllUsers()
		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "users": users, "count": len(users)})
	}
}

func HandleBanUser(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		adminID := extractUserID(r)
		if adminID == "" {
			writeJSON(w, http.StatusUnauthorized, map[string]interface{}{"code": 1002, "message": "Unauthorized"})
			return
		}

		var req struct {
			UserID string `json:"user_id"`
			Reason string `json:"reason"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.BanUser(req.UserID, req.Reason, adminID); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to ban user"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "User banned"})
	}
}

func HandleUnbanUser(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		var req struct {
			UserID string `json:"user_id"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.UnbanUser(req.UserID); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to unban user"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "User unbanned"})
	}
}

func HandleGetStats(store *storage.Storage, cfg *config.Config) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":         0,
			"total_users":  store.GetTotalCount(),
			"online_users": store.GetOnlineCount(),
			"server_name":  cfg.Server.Name,
			"version":      cfg.Version.Current,
		})
	}
}

func HandleGetConfig(cfg *config.Config) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":        0,
			"server_name": cfg.Server.Name,
			"version":     cfg.Version.Current,
			"chat_host":   cfg.ChatServer.Host,
			"chat_port":   cfg.ChatServer.Port,
			"chat_proto":  cfg.ChatServer.Protocol,
		})
	}
}
