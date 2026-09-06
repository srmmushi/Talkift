package handlers

import (
	"time"

	"talkift_server/internal/models"
)

const PublicGroupID = "public-group"

func InitPublicGroup(store interface {
	SaveConversation(conv *models.Conversation) error
	LoadConversation(id string) (*models.Conversation, error)
}) error {
	existing, err := store.LoadConversation(PublicGroupID)
	if err == nil && existing != nil {
		return nil
	}

	conv := &models.Conversation{
		ID:        PublicGroupID,
		Name:      "Public Group",
		Type:      models.ConversationTypeGroup,
		OwnerID:   "system",
		Members:   []string{},
		CreatedAt: time.Now(),
		IsPublic:  true,
	}

	if err := store.SaveConversation(conv); err != nil {
		return err
	}

	return nil
}
