package handler

import (
	"encoding/json"
	"net/http"

	"official-server/internal/config"
)

type VersionResponse struct {
	Code         int                  `json:"code"`
	ServerName   string               `json:"server_name"`
	UniqueId     string               `json:"unique_id"`
	Version      *config.VersionConfig `json:"version,omitempty"`
	Message      string               `json:"message,omitempty"`
}

func HandleVersion(cfg *config.Config) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, VersionResponse{
				Code: 1004, Message: "Method not allowed",
			})
			return
		}

		writeJSON(w, http.StatusOK, VersionResponse{
			Code:       0,
			ServerName: cfg.Server.Name,
			UniqueId:   cfg.Server.UniqueId,
			Version:    &cfg.Version,
		})
	}
}

func HandleServerInfo(cfg *config.Config) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{
				"code": 1004, "message": "Method not allowed",
			})
			return
		}

		json.NewEncoder(w).Encode(map[string]interface{}{
			"code":       0,
			"server_name": cfg.Server.Name,
			"unique_id":  cfg.Server.UniqueId,
			"version":    cfg.Version.Current,
			"min_client": cfg.Version.MinClient,
			"status":     "online",
		})
	}
}
