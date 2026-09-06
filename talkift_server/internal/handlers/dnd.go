package handlers

import (
	"encoding/json"
	"net/http"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
)

func (h *Handlers) HandleDnd(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.DndPayload
	if err := json.NewDecoder(r.Body).Decode(&payload); err != nil {
		h.writeJSON(w, http.StatusBadRequest, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "invalid request body",
		})
		return
	}

	user, err := h.store.LoadUser(userID)
	if err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	found := false
	for _, id := range user.MutedConversations {
		if id == payload.ConversationID {
			found = true
			break
		}
	}

	if payload.Enabled && !found {
		user.MutedConversations = append(user.MutedConversations, payload.ConversationID)
	} else if !payload.Enabled && found {
		newMuted := make([]string, 0, len(user.MutedConversations)-1)
		for _, id := range user.MutedConversations {
			if id != payload.ConversationID {
				newMuted = append(newMuted, id)
			}
		}
		user.MutedConversations = newMuted
	}

	if err := h.store.SaveUser(user); err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	status := "enabled"
	if !payload.Enabled {
		status = "disabled"
	}

	h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
		Success: true,
		Code:    models.CodeSuccess,
		Message: "DND " + status + " for conversation " + payload.ConversationID,
	})
}
