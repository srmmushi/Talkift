package handler

import (
	"net/http"

	"official-server/internal/config"
	"official-server/internal/storage"
)

func HandleVersion(cfg *config.Config) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":       0,
			"server_name": cfg.Server.Name,
			"version":    cfg.Version.Current,
			"min_client": cfg.Version.MinClient,
			"update_url": cfg.Version.UpdateUrl,
		})
	}
}

func HandleServerInfo(cfg *config.Config, store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":         0,
			"server_name":  cfg.Server.Name,
			"version":      cfg.Version.Current,
			"min_client":   cfg.Version.MinClient,
			"chat_host":    cfg.ChatServer.Host,
			"chat_port":    cfg.ChatServer.Port,
			"chat_proto":   cfg.ChatServer.Protocol,
			"status":       "online",
			"online_users": store.GetOnlineCount(),
			"total_users":  store.GetTotalCount(),
		})
	}
}

func HandleHealthCheck(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		writeJSON(w, http.StatusOK, map[string]interface{}{
			"status":       "healthy",
			"online_users": store.GetOnlineCount(),
			"total_users":  store.GetTotalCount(),
		})
	}
}

func HandlePing(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "pong"})
}
