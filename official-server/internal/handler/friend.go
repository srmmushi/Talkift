package handler

import (
	"net/http"

	"official-server/internal/storage"
)

func HandleSendFriendRequest(store *storage.Storage) http.HandlerFunc {
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
			TargetUsername string `json:"target_username"`
			Message        string `json:"message"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		target, exists := store.GetUserByUsername(req.TargetUsername)
		if !exists {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "User not found"})
			return
		}

		writeJSON(w, http.StatusCreated, map[string]interface{}{
			"code":    0,
			"message": "Friend request sent",
			"target":  target.Username,
		})
	}
}

func HandleAcceptFriendRequest() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Friend request accepted"})
	}
}

func HandleRejectFriendRequest() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Friend request rejected"})
	}
}

func HandleGetFriendRequests() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		userID := extractUserID(r)
		if userID == "" {
			writeJSON(w, http.StatusUnauthorized, map[string]interface{}{"code": 1002, "message": "Unauthorized"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "requests": []interface{}{}, "count": 0})
	}
}
