package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

func (h *Handlers) HandleCreateConversation(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.CreateConversationPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	if payload.TargetUserID == "" {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "target_user_id is required",
		})
		return
	}

	targetUser, err := h.store.LoadUser(payload.TargetUserID)
	if err != nil {
		h.writeJSON(w, http.StatusNotFound, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeUserNotFound,
			Message: models.ErrorMessage(models.CodeUserNotFound),
		})
		return
	}

	publicGroup, err := h.store.LoadConversation(PublicGroupID)
	if err != nil {
		h.writeJSON(w, http.StatusInternalServerError, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeInternalError,
			Message: "public group not found",
		})
		return
	}

	targetInPublic := false
	for _, m := range publicGroup.Members {
		if m == payload.TargetUserID {
			targetInPublic = true
			break
		}
	}
	if !targetInPublic {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeTargetNotInPublic,
			Message: models.ErrorMessage(models.CodeTargetNotInPublic),
		})
		return
	}

	creatorInPublic := false
	for _, m := range publicGroup.Members {
		if m == userID {
			creatorInPublic = true
			break
		}
	}
	if !creatorInPublic {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeTargetNotInPublic,
			Message: "you are not in the public group",
		})
		return
	}

	convName := payload.ConvName
	if convName == "" {
		creator, _ := h.store.LoadUser(userID)
		creatorName := "User"
		if creator != nil {
			creatorName = creator.Username
		}
		convName = creatorName + " & " + targetUser.Username
	}

	conv := &models.Conversation{
		ID:        utils.NewUUID(),
		Name:      convName,
		Type:      models.ConversationTypeConversation,
		OwnerID:   userID,
		Members:   []string{userID, payload.TargetUserID},
		CreatedAt: time.Now(),
		IsPublic:  false,
	}

	if err := h.store.SaveConversation(conv); err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	h.writeJSON(w, http.StatusCreated, models.CreateGroupResponsePayload{
		Success:      true,
		Code:         models.CodeSuccess,
		Conversation: conv,
	})

	h.hub.SendToUserID(payload.TargetUserID, models.WSMessage{
		Type: string(models.MsgTypeSystem),
		Payload: map[string]interface{}{
			"message":         "New conversation started: " + conv.Name,
			"conversation_id": conv.ID,
		},
	})
}
