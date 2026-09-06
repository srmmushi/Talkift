package storage

import (
	"encoding/json"
	"errors"
	"os"
	"sync"
	"time"

	"loginserver/internal/model"

	"github.com/google/uuid"
	"golang.org/x/crypto/bcrypt"
)

var (
	ErrUsernameExists = errors.New("username already exists")
	ErrUserNotFound  = errors.New("user not found")
	ErrWrongPassword = errors.New("invalid password")
)

type Storage struct {
	mu       sync.RWMutex
	users    map[string]*model.User
	filePath string
}

func New(filePath string) *Storage {
	s := &Storage{
		users:    make(map[string]*model.User),
		filePath: filePath,
	}
	s.load()
	return s
}

func (s *Storage) load() {
	data, err := os.ReadFile(s.filePath)
	if err != nil {
		return
	}

	var users []*model.User
	if err := json.Unmarshal(data, &users); err != nil {
		return
	}

	for _, u := range users {
		s.users[u.Username] = u
	}
}

func (s *Storage) save() error {
	users := make([]*model.User, 0, len(s.users))
	for _, u := range s.users {
		users = append(users, u)
	}

	data, err := json.MarshalIndent(users, "", "  ")
	if err != nil {
		return err
	}

	return os.WriteFile(s.filePath, data, 0644)
}

func (s *Storage) CreateUser(username, password string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	if _, exists := s.users[username]; exists {
		return nil, ErrUsernameExists
	}

	hashed, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		return nil, err
	}

	user := &model.User{
		ID:             uuid.New().String(),
		Username:       username,
		PasswordHash:   string(hashed),
		RegisterMethod: "local",
		CreatedAt:      time.Now(),
		LastLoginAt:    time.Now(),
	}

	s.users[username] = user

	if err := s.save(); err != nil {
		delete(s.users, username)
		return nil, err
	}

	return user, nil
}

func (s *Storage) AuthenticateUser(username, password string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	user, exists := s.users[username]
	if !exists {
		return nil, ErrUserNotFound
	}

	if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(password)); err != nil {
		return nil, ErrWrongPassword
	}

	user.LastLoginAt = time.Now()
	s.save()

	return user, nil
}

func (s *Storage) GetUser(username string) (*model.User, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	user, exists := s.users[username]
	return user, exists
}

func (s *Storage) UserExists(username string) bool {
	s.mu.RLock()
	defer s.mu.RUnlock()

	_, exists := s.users[username]
	return exists
}

func (s *Storage) GetUserByID(id string) (*model.User, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	for _, u := range s.users {
		if u.ID == id {
			return u, true
		}
	}
	return nil, false
}
