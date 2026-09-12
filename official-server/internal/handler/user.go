package handler

import (
	"net/http"

	"official-server/internal/storage"
)

func HandleGetProfile(store *storage.Storage) http.HandlerFunc {
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
			"avatar":   user.Avatar,
			"bio":      user.Bio,
			"status":   user.Status,
		})
	}
}

func HandleUpdateProfile(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPut {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		userID := extractUserID(r)
		if userID == "" {
			writeJSON(w, http.StatusUnauthorized, map[string]interface{}{"code": 1002, "message": "Unauthorized"})
			return
		}

		var updates map[string]interface{}
		if err := decodeBody(r, &updates); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.UpdateUser(userID, updates); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to update profile"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Profile updated"})
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
		if err := decodeBody(r, &req); err != nil {
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

func HandleSetStatus(store *storage.Storage) http.HandlerFunc {
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
			Status string `json:"status"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.UpdateUser(userID, map[string]interface{}{"status": req.Status}); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to update status"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Status updated"})
	}
}

func HandleSearchUsers(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		query := r.URL.Query().Get("q")
		if query == "" {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Query parameter 'q' is required"})
			return
		}

		users := store.SearchUsers(query)
		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "users": users, "count": len(users)})
	}
}

func HandleChangePassword(store *storage.Storage) http.HandlerFunc {
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
			OldPassword string `json:"old_password"`
			NewPassword string `json:"new_password"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if err := store.ChangePassword(userID, req.OldPassword, req.NewPassword); err != nil {
			code, msg := 1005, "Failed to change password"
			if err == storage.ErrWrongPassword {
				code, msg = 1002, "Invalid old password"
			}
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": code, "message": msg})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Password changed"})
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

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "User blocked"})
	}
}
