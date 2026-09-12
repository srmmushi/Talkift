package handler

import (
	"net/http"
)

func HandleGetNotifications() http.HandlerFunc {
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

		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":          0,
			"notifications": []interface{}{},
			"unread_count":  0,
			"count":         0,
		})
	}
}

func HandleMarkNotificationRead() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Notification marked as read"})
	}
}

func HandleMarkAllNotificationsRead() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "All notifications marked as read"})
	}
}

func HandleDeleteNotification() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodDelete {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Notification deleted"})
	}
}
