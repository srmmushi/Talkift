package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

func (h *Handlers) HandleChat(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.ChatPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	if payload.ConversationID == "" || payload.Content == "" {
		h.writeJSON(w, http.StatusBadRequest, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "conversation_id and content are required",
		})
		return
	}

	conv, err := h.store.LoadConversation(payload.ConversationID)
	if err != nil {
		h.writeJSON(w, http.StatusNotFound, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeConversationNotFound,
			Message: models.ErrorMessage(models.CodeConversationNotFound),
		})
		return
	}

	isMember := false
	for _, m := range conv.Members {
		if m == userID {
			isMember = true
			break
		}
	}
	if !isMember {
		h.writeJSON(w, http.StatusForbidden, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeNotMember,
			Message: models.ErrorMessage(models.CodeNotMember),
		})
		return
	}

	isBanned := false
	for _, b := range conv.BannedMembers {
		if b == userID {
			isBanned = true
			break
		}
	}
	if isBanned {
		h.writeJSON(w, http.StatusForbidden, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeBanned,
			Message: models.ErrorMessage(models.CodeBanned),
		})
		return
	}

	senderName := ""
	sender, err := h.store.LoadUser(userID)
	if err == nil {
		senderName = sender.Username
	}

	msgType := payload.Type
	if msgType == "" {
		msgType = "text"
	}

	msg := &models.Message{
		ID:             utils.NewUUID(),
		ConversationID: payload.ConversationID,
		SenderID:       userID,
		SenderName:     senderName,
		Content:        payload.Content,
		Type:           msgType,
		Timestamp:      time.Now(),
	}

	if err := h.store.SaveMessage(msg); err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	h.hub.SendToUserIDs(conv.Members, models.WSMessage{
		Type:    string(models.MsgTypeChat),
		Payload: msg,
	})

	h.writeJSON(w, http.StatusCreated, models.ChatResponsePayload{
		Success: true,
		Code:    models.CodeSuccess,
	})
}

func (h *Handlers) HandleLoadHistory(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.LoadHistoryPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	if payload.ConversationID == "" {
		h.writeJSON(w, http.StatusBadRequest, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "conversation_id is required",
		})
		return
	}

	conv, err := h.store.LoadConversation(payload.ConversationID)
	if err != nil {
		h.writeJSON(w, http.StatusNotFound, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeConversationNotFound,
			Message: models.ErrorMessage(models.CodeConversationNotFound),
		})
		return
	}

	isMember := false
	for _, m := range conv.Members {
		if m == userID {
			isMember = true
			break
		}
	}
	if !isMember {
		h.writeJSON(w, http.StatusForbidden, models.ChatResponsePayload{
			Success: false,
			Code:    models.CodeNotMember,
			Message: models.ErrorMessage(models.CodeNotMember),
		})
		return
	}

	limit := payload.Limit
	if limit <= 0 || limit > 100 {
		limit = 50
	}

	msgs, err := h.store.LoadMessagesBefore(payload.ConversationID, limit, payload.Before)
	if err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	h.writeJSON(w, http.StatusOK, map[string]interface{}{
		"success":  true,
		"code":     models.CodeSuccess,
		"messages": msgs,
	})
}
