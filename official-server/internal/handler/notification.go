package handler

import (
	"encoding/json"
	"net/http"
	"time"

	"official-server/internal/model"
)

type NotificationStore struct {
	notifications map[string]*model.Notification
}

func NewNotificationStore() *NotificationStore {
	return &NotificationStore{
		notifications: make(map[string]*model.Notification),
	}
}

func (s *NotificationStore) Create(userID, title, body, notifType string) *model.Notification {
	id := time.Now().Format("20060102150405") + "-" + userID[:8]
	notif := &model.Notification{
		ID:        id,
		UserID:    userID,
		Title:     title,
		Body:      body,
		Type:      notifType,
		Read:      false,
		CreatedAt: time.Now(),
	}
	s.notifications[id] = notif
	return notif
}

func (s *NotificationStore) GetByUser(userID string) []*model.Notification {
	var result []*model.Notification
	for _, n := range s.notifications {
		if n.UserID == userID {
			result = append(result, n)
		}
	}
	return result
}

func (s *NotificationStore) GetUnreadCount(userID string) int {
	count := 0
	for _, n := range s.notifications {
		if n.UserID == userID && !n.Read {
			count++
		}
	}
	return count
}

func (s *NotificationStore) MarkRead(id string) bool {
	if n, ok := s.notifications[id]; ok {
		n.Read = true
		return true
	}
	return false
}

func (s *NotificationStore) MarkAllRead(userID string) {
	for _, n := range s.notifications {
		if n.UserID == userID {
			n.Read = true
		}
	}
}

func (s *NotificationStore) Delete(id string) bool {
	if _, ok := s.notifications[id]; ok {
		delete(s.notifications, id)
		return true
	}
	return false
}

var notifStore = NewNotificationStore()

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

		notifications := notifStore.GetByUser(userID)
		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":          0,
			"notifications": notifications,
			"unread_count":  notifStore.GetUnreadCount(userID),
			"count":         len(notifications),
		})
	}
}

func HandleMarkNotificationRead() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		var req struct {
			NotificationID string `json:"notification_id"`
		}
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if !notifStore.MarkRead(req.NotificationID) {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Notification not found"})
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

		userID := extractUserID(r)
		if userID == "" {
			writeJSON(w, http.StatusUnauthorized, map[string]interface{}{"code": 1002, "message": "Unauthorized"})
			return
		}

		notifStore.MarkAllRead(userID)
		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "All notifications marked as read"})
	}
}

func HandleDeleteNotification() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodDelete {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		notifID := r.URL.Query().Get("id")
		if notifID == "" {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Notification ID is required"})
			return
		}

		if !notifStore.Delete(notifID) {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Notification not found"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Notification deleted"})
	}
}
