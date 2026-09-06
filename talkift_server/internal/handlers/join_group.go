package handlers

import (
	"encoding/json"
	"net/http"

	"talkift_server/internal/middleware"
	"talkift_server/internal/models"
)

func (h *Handlers) HandleJoinGroupRequest(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	userID := middleware.GetUserID(r)

	var payload models.JoinGroupPayload
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

	if conv.Type != models.ConversationTypeGroup {
		h.writeJSON(w, http.StatusBadRequest, models.JoinGroupResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "not a group",
		})
		return
	}

	for _, m := range conv.Members {
		if m == userID {
			h.writeJSON(w, http.StatusConflict, models.JoinGroupResponsePayload{
				Success: false,
				Code:    models.CodeAlreadyMember,
				Message: models.ErrorMessage(models.CodeAlreadyMember),
			})
			return
		}
	}

	user, err := h.store.LoadUser(userID)
	if err != nil {
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	h.hub.SendToUserID(conv.OwnerID, models.WSMessage{
		Type: string(models.MsgTypeJoinGroupRequest),
		Payload: models.JoinGroupRequestPayload{
			ConversationID: payload.ConversationID,
			UserID:         userID,
			Username:       user.Username,
		},
	})

	h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
		Success:        true,
		Code:           models.CodeSuccess,
		ConversationID: payload.ConversationID,
		Message:        "join request sent to group owner",
	})
}

func (h *Handlers) HandleJoinGroupResponse(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	ownerID := middleware.GetUserID(r)

	var payload models.JoinGroupResponsePayload
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
			Message: "only group owner can accept join requests",
		})
		return
	}

	if payload.Accepted {
		conv.Members = append(conv.Members, payload.UserID)
		if err := h.store.SaveConversation(conv); err != nil {
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}

		h.hub.SendToUserID(payload.UserID, models.WSMessage{
			Type: string(models.MsgTypeJoinGroupResponse),
			Payload: models.JoinGroupResponsePayload{
				Success:        true,
				Code:           models.CodeSuccess,
				ConversationID: payload.ConversationID,
				Accepted:       true,
				Message:        "you have been accepted into " + conv.Name,
			},
		})

		for _, mid := range conv.Members {
			if mid == payload.UserID || mid == ownerID {
				continue
			}
			h.hub.SendToUserID(mid, models.WSMessage{
				Type: string(models.MsgTypeSystem),
				Payload: map[string]interface{}{
					"message": "A new member has joined " + conv.Name,
				},
			})
		}
	} else {
		h.hub.SendToUserID(payload.UserID, models.WSMessage{
			Type: string(models.MsgTypeJoinGroupResponse),
			Payload: models.JoinGroupResponsePayload{
				Success:        false,
				Code:           models.CodeUnauthorized,
				ConversationID: payload.ConversationID,
				Accepted:       false,
				Message:        "your join request was rejected",
			},
		})
	}

	h.writeJSON(w, http.StatusOK, models.JoinGroupResponsePayload{
		Success:  true,
		Code:     models.CodeSuccess,
		Accepted: payload.Accepted,
	})
}
