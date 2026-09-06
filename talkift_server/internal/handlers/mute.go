package handlers

import (
	"encoding/json"
	"net/http"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
)

func (h *Handlers) HandleMute(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	ownerID := middleware.GetUserID(r)

	var payload models.MutePayload
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

	if conv.OwnerID != ownerID {
		h.writeJSON(w, http.StatusForbidden, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeUnauthorized,
			Message: "only group owner can mute/unmute members",
		})
		return
	}

	targetBanned := false
	for _, b := range conv.BannedMembers {
		if b == payload.UserID {
			targetBanned = true
			break
		}
	}

	if targetBanned {
		newBanned := make([]string, 0, len(conv.BannedMembers)-1)
		for _, b := range conv.BannedMembers {
			if b != payload.UserID {
				newBanned = append(newBanned, b)
			}
		}
		conv.BannedMembers = newBanned

		h.hub.SendToUserID(payload.UserID, models.WSMessage{
			Type: string(models.MsgTypeMuteResponse),
			Payload: models.JoinGroupResponsePayload{
				Success: true,
				Code:    models.CodeSuccess,
				Message: "you have been unmuted in " + conv.Name,
			},
		})
	} else {
		conv.BannedMembers = append(conv.BannedMembers, payload.UserID)

		h.hub.SendToUserID(payload.UserID, models.WSMessage{
			Type: string(models.MsgTypeMuteResponse),
			Payload: models.JoinGroupResponsePayload{
				Success: false,
				Code:    models.CodeBanned,
				Message: "you have been muted in " + conv.Name,
			},
		})
	}

	if err := h.store.SaveConversation(conv); err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
		Success:  true,
		Code:     models.CodeSuccess,
		Accepted: !targetBanned,
		Message:  "mute status updated",
	})
}
