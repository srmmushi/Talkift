package storage

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"sync"

	"talkift_server/internal/models"
)

type Storage interface {
	SaveUser(user *models.User) error
	LoadUser(id string) (*models.User, error)
	LoadUserByUsername(username string) (*models.User, error)
	SaveMessage(msg *models.Message) error
	LoadMessages(conversationID string, limit, offset int) ([]*models.Message, error)
	LoadMessagesBefore(conversationID string, limit int, beforeID string) ([]*models.Message, error)
	DeleteMessages(conversationID string) error
	SaveConversation(conv *models.Conversation) error
	LoadConversation(id string) (*models.Conversation, error)
	DeleteConversation(id string, retainMessages bool) error
	ListConversations(userID string) ([]*models.Conversation, error)
}

type LocalStorage struct {
	mu        sync.RWMutex
	basePath  string
	users     map[string]*models.User
	userIndex map[string]string
	convs     map[string]*models.Conversation
}

func NewLocalStorage(basePath string) (*LocalStorage, error) {
	dirs := []string{
		filepath.Join(basePath, "reg", "users"),
		filepath.Join(basePath, "messages"),
		filepath.Join(basePath, "conversations"),
		filepath.Join(basePath, "logs"),
	}
	for _, d := range dirs {
		if err := os.MkdirAll(d, 0755); err != nil {
			return nil, fmt.Errorf("create storage dirs: %w", err)
		}
	}

	s := &LocalStorage{
		basePath:  basePath,
		users:     make(map[string]*models.User),
		userIndex: make(map[string]string),
		convs:     make(map[string]*models.Conversation),
	}

	if err := s.loadAll(); err != nil {
		return nil, err
	}

	return s, nil
}

func (s *LocalStorage) loadAll() error {
	s.loadUsers()
	s.loadConversations()
	return s.loadUserIndex()
}

func (s *LocalStorage) loadUsers() {
	userDir := filepath.Join(s.basePath, "reg", "users")
	entries, err := os.ReadDir(userDir)
	if err != nil {
		return
	}
	for _, e := range entries {
		if e.IsDir() || filepath.Ext(e.Name()) != ".json" {
			continue
		}
		var user models.User
		data, err := os.ReadFile(filepath.Join(userDir, e.Name()))
		if err != nil {
			continue
		}
		if json.Unmarshal(data, &user) == nil {
			s.users[user.ID] = &user
			s.userIndex[user.Username] = user.ID
		}
	}
}

func (s *LocalStorage) loadUserIndex() error {
	indexPath := filepath.Join(s.basePath, "reg", "users_index.json")
	data, err := os.ReadFile(indexPath)
	if err != nil {
		if os.IsNotExist(err) {
			return s.rebuildUserIndex()
		}
		return err
	}

	var index map[string]string
	if err := json.Unmarshal(data, &index); err != nil {
		return s.rebuildUserIndex()
	}

	s.userIndex = index
	return nil
}

func (s *LocalStorage) rebuildUserIndex() error {
	s.userIndex = make(map[string]string)
	for id, user := range s.users {
		s.userIndex[user.Username] = id
	}
	return s.saveUserIndex()
}

func (s *LocalStorage) saveUserIndex() error {
	indexPath := filepath.Join(s.basePath, "reg", "users_index.json")
	data, err := json.MarshalIndent(s.userIndex, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(indexPath, data, 0644)
}

func (s *LocalStorage) loadConversations() {
	convDir := filepath.Join(s.basePath, "conversations")
	entries, err := os.ReadDir(convDir)
	if err != nil {
		return
	}
	for _, e := range entries {
		if e.IsDir() || filepath.Ext(e.Name()) != ".json" {
			continue
		}
		var conv models.Conversation
		data, err := os.ReadFile(filepath.Join(convDir, e.Name()))
		if err != nil {
			continue
		}
		if json.Unmarshal(data, &conv) == nil {
			s.convs[conv.ID] = &conv
		}
	}
}

func (s *LocalStorage) SaveUser(user *models.User) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.users[user.ID] = user
	s.userIndex[user.Username] = user.ID

	data, err := json.MarshalIndent(user, "", "  ")
	if err != nil {
		return err
	}
	path := filepath.Join(s.basePath, "reg", "users", user.ID+".json")
	if err := os.WriteFile(path, data, 0644); err != nil {
		return err
	}

	return s.saveUserIndex()
}

func (s *LocalStorage) LoadUser(id string) (*models.User, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	user, ok := s.users[id]
	if !ok {
		return nil, fmt.Errorf("user %s not found", id)
	}
	return user, nil
}

func (s *LocalStorage) LoadUserByUsername(username string) (*models.User, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	userID, ok := s.userIndex[username]
	if !ok {
		return nil, fmt.Errorf("user %s not found", username)
	}

	user, ok := s.users[userID]
	if !ok {
		return nil, fmt.Errorf("user %s not found", username)
	}
	return user, nil
}

func (s *LocalStorage) SaveMessage(msg *models.Message) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	dir := filepath.Join(s.basePath, "messages")
	path := filepath.Join(dir, msg.ConversationID+".json")

	var msgs []*models.Message
	data, err := os.ReadFile(path)
	if err == nil {
		_ = json.Unmarshal(data, &msgs)
	}

	msgs = append(msgs, msg)
	data, err = json.MarshalIndent(msgs, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(path, data, 0644)
}

func (s *LocalStorage) LoadMessages(conversationID string, limit, offset int) ([]*models.Message, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	path := filepath.Join(s.basePath, "messages", conversationID+".json")
	data, err := os.ReadFile(path)
	if err != nil {
		if os.IsNotExist(err) {
			return []*models.Message{}, nil
		}
		return nil, err
	}

	var msgs []*models.Message
	if err := json.Unmarshal(data, &msgs); err != nil {
		return nil, err
	}

	total := len(msgs)
	if offset >= total {
		return []*models.Message{}, nil
	}

	end := offset + limit
	if end > total {
		end = total
	}

	return msgs[offset:end], nil
}

func (s *LocalStorage) LoadMessagesBefore(conversationID string, limit int, beforeID string) ([]*models.Message, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	path := filepath.Join(s.basePath, "messages", conversationID+".json")
	data, err := os.ReadFile(path)
	if err != nil {
		if os.IsNotExist(err) {
			return []*models.Message{}, nil
		}
		return nil, err
	}

	var msgs []*models.Message
	if err := json.Unmarshal(data, &msgs); err != nil {
		return nil, err
	}

	if beforeID == "" {
		total := len(msgs)
		if limit <= 0 {
			limit = 50
		}
		start := total - limit
		if start < 0 {
			start = 0
		}
		return msgs[start:total], nil
	}

	idx := -1
	for i, m := range msgs {
		if m.ID == beforeID {
			idx = i
			break
		}
	}

	if idx <= 0 {
		return []*models.Message{}, nil
	}

	if limit <= 0 {
		limit = 50
	}

	start := idx - limit
	if start < 0 {
		start = 0
	}

	return msgs[start:idx], nil
}

func (s *LocalStorage) DeleteMessages(conversationID string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	path := filepath.Join(s.basePath, "messages", conversationID+".json")
	if err := os.Remove(path); err != nil && !os.IsNotExist(err) {
		return err
	}
	return nil
}

func (s *LocalStorage) SaveConversation(conv *models.Conversation) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.convs[conv.ID] = conv
	data, err := json.MarshalIndent(conv, "", "  ")
	if err != nil {
		return err
	}
	path := filepath.Join(s.basePath, "conversations", conv.ID+".json")
	return os.WriteFile(path, data, 0644)
}

func (s *LocalStorage) LoadConversation(id string) (*models.Conversation, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	conv, ok := s.convs[id]
	if !ok {
		return nil, fmt.Errorf("conversation %s not found", id)
	}
	return conv, nil
}

func (s *LocalStorage) DeleteConversation(id string, retainMessages bool) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	delete(s.convs, id)
	path := filepath.Join(s.basePath, "conversations", id+".json")
	if err := os.Remove(path); err != nil && !os.IsNotExist(err) {
		return err
	}

	if !retainMessages {
		msgPath := filepath.Join(s.basePath, "messages", id+".json")
		return os.Remove(msgPath)
	}

	return nil
}

func (s *LocalStorage) ListConversations(userID string) ([]*models.Conversation, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var result []*models.Conversation
	for _, conv := range s.convs {
		for _, member := range conv.Members {
			if member == userID {
				result = append(result, conv)
				break
			}
		}
	}
	return result, nil
}
