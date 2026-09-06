package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

func (h *Handlers) HandleCreateGroup(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.CreateGroupPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	if payload.GroupName == "" {
		h.writeJSON(w, http.StatusBadRequest, models.CreateGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "group_name is required",
		})
		return
	}

	members := []string{userID}
	for _, mid := range payload.MemberIDs {
		if mid == userID {
			continue
		}
		if _, err := h.store.LoadUser(mid); err == nil {
			members = append(members, mid)
		}
	}

	conv := &models.Conversation{
		ID:        utils.NewUUID(),
		Name:      payload.GroupName,
		Type:      models.ConversationTypeGroup,
		OwnerID:   userID,
		Members:   members,
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

	for _, mid := range members {
		if mid == userID {
			continue
		}
		h.hub.SendToUserID(mid, models.WSMessage{
			Type: string(models.MsgTypeSystem),
			Payload: map[string]interface{}{
				"message":  "You have been added to group " + conv.Name,
				"group_id": conv.ID,
			},
		})
	}
}
