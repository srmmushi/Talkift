package handler

import (
	"encoding/json"
	"net/http"
	"time"

	"official-server/internal/model"
	"official-server/internal/storage"
)

type FriendRequestStore struct {
	requests map[string]*model.FriendRequest
}

func NewFriendRequestStore() *FriendRequestStore {
	return &FriendRequestStore{
		requests: make(map[string]*model.FriendRequest),
	}
}

func (s *FriendRequestStore) Create(from, to, message string) *model.FriendRequest {
	id := time.Now().Format("20060102150405") + "-" + from[:8]
	req := &model.FriendRequest{
		ID:        id,
		FromUser:  from,
		ToUser:    to,
		Status:    "pending",
		Message:   message,
		CreatedAt: time.Now(),
	}
	s.requests[id] = req
	return req
}

func (s *FriendRequestStore) GetByID(id string) *model.FriendRequest {
	return s.requests[id]
}

func (s *FriendRequestStore) GetByUser(userID string) []*model.FriendRequest {
	var result []*model.FriendRequest
	for _, req := range s.requests {
		if req.ToUser == userID || req.FromUser == userID {
			result = append(result, req)
		}
	}
	return result
}

func (s *FriendRequestStore) GetPending(userID string) []*model.FriendRequest {
	var result []*model.FriendRequest
	for _, req := range s.requests {
		if req.ToUser == userID && req.Status == "pending" {
			result = append(result, req)
		}
	}
	return result
}

func (s *FriendRequestStore) Accept(id string) bool {
	if req, ok := s.requests[id]; ok {
		req.Status = "accepted"
		return true
	}
	return false
}

func (s *FriendRequestStore) Reject(id string) bool {
	if req, ok := s.requests[id]; ok {
		req.Status = "rejected"
		return true
	}
	return false
}

var friendStore = NewFriendRequestStore()

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
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		target, exists := store.GetUser(req.TargetUsername)
		if !exists {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "User not found"})
			return
		}

		fr := friendStore.Create(userID, target.ID, req.Message)
		writeJSON(w, http.StatusCreated, map[string]interface{}{
			"code":    0,
			"message": "Friend request sent",
			"request": fr,
		})
	}
}

func HandleAcceptFriendRequest() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		var req struct {
			RequestID string `json:"request_id"`
		}
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if !friendStore.Accept(req.RequestID) {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Request not found"})
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

		var req struct {
			RequestID string `json:"request_id"`
		}
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		if !friendStore.Reject(req.RequestID) {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Request not found"})
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

		requests := friendStore.GetPending(userID)
		writeJSON(w, http.StatusOK, map[string]interface{}{
			"code":     0,
			"requests": requests,
			"count":    len(requests),
		})
	}
}
