package handlers

import (
	"encoding/json"
	"net/http"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
)

func (h *Handlers) HandleLeaveGroup(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.LeaveGroupPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	conv, err := h.store.LoadConversation(payload.ConversationID)
	if err != nil {
		h.writeJSON(w, http.StatusNotFound, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeConversationNotFound,
			Message: models.ErrorMessage(models.CodeConversationNotFound),
		})
		return
	}

	if conv.IsPublic {
		h.writeJSON(w, http.StatusForbidden, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeCannotLeavePublic,
			Message: models.ErrorMessage(models.CodeCannotLeavePublic),
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
		h.writeJSON(w, http.StatusForbidden, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeNotMember,
			Message: models.ErrorMessage(models.CodeNotMember),
		})
		return
	}

	if conv.OwnerID == userID {
		remainingMembers := make([]string, len(conv.Members))
		copy(remainingMembers, conv.Members)

		for _, m := range remainingMembers {
			h.hub.SendToUserID(m, models.WSMessage{
				Type: string(models.MsgTypeSystem),
				Payload: map[string]interface{}{
					"message":  "Group " + conv.Name + " has been dissolved by the owner",
					"group_id": conv.ID,
				},
			})
		}

		retainMessages := h.cfg.Storage.RetainMessages
		if err := h.store.DeleteConversation(conv.ID, retainMessages); err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}

		h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
			Success: true,
			Code:    models.CodeSuccess,
			Message: "group dissolved",
		})
		return
	}

	newMembers := make([]string, 0, len(conv.Members)-1)
	for _, m := range conv.Members {
		if m != userID {
			newMembers = append(newMembers, m)
		}
	}
	conv.Members = newMembers

	if err := h.store.SaveConversation(conv); err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	for _, m := range conv.Members {
		h.hub.SendToUserID(m, models.WSMessage{
			Type: string(models.MsgTypeSystem),
			Payload: map[string]interface{}{
				"message": "A member has left " + conv.Name,
			},
		})
	}

	h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
		Success: true,
		Code:    models.CodeSuccess,
		Message: "you have left the group",
	})
}
