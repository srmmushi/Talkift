package storage

import (
	"encoding/json"
	"errors"
	"os"
	"sync"
	"time"

	"official-server/internal/model"

	"github.com/google/uuid"
	"golang.org/x/crypto/bcrypt"
)

var (
	ErrUsernameExists = errors.New("username already exists")
	ErrEmailExists    = errors.New("email already exists")
	ErrUserNotFound   = errors.New("user not found")
	ErrWrongPassword  = errors.New("invalid password")
	ErrUserBanned     = errors.New("user is banned")
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

func (s *Storage) CreateUser(username, password, email, registerMethod string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	if _, exists := s.users[username]; exists {
		return nil, ErrUsernameExists
	}

	if email != "" {
		for _, u := range s.users {
			if u.Email == email {
				return nil, ErrEmailExists
			}
		}
	}

	hashed, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		return nil, err
	}

	if registerMethod == "" {
		registerMethod = "official"
	}

	user := &model.User{
		ID:             uuid.New().String(),
		Username:       username,
		Email:          email,
		PasswordHash:   string(hashed),
		RegisterMethod: registerMethod,
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

	if user.IsBanned {
		return nil, ErrUserBanned
	}

	if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(password)); err != nil {
		return nil, ErrWrongPassword
	}

	user.LastLoginAt = time.Now()
	s.save()

	return user, nil
}

func (s *Storage) AuthenticateByEmail(email, password string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.Email == email {
			if user.IsBanned {
				return nil, ErrUserBanned
			}
			if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(password)); err != nil {
				return nil, ErrWrongPassword
			}
			user.LastLoginAt = time.Now()
			s.save()
			return user, nil
		}
	}

	return nil, ErrUserNotFound
}

func (s *Storage) GetUser(username string) (*model.User, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	user, exists := s.users[username]
	return user, exists
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

func (s *Storage) UserExists(username string) bool {
	s.mu.RLock()
	defer s.mu.RUnlock()

	_, exists := s.users[username]
	return exists
}

func (s *Storage) EmailExists(email string) bool {
	s.mu.RLock()
	defer s.mu.RUnlock()

	for _, u := range s.users {
		if u.Email == email {
			return true
		}
	}
	return false
}

func (s *Storage) UpdateUser(id string, updates map[string]interface{}) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.ID == id {
			if v, ok := updates["email"].(string); ok {
				user.Email = v
			}
			if v, ok := updates["avatar"].(string); ok {
				user.Avatar = v
			}
			if v, ok := updates["bio"].(string); ok {
				user.Bio = v
			}
			if v, ok := updates["status"].(string); ok {
				user.Status = v
			}
			return s.save()
		}
	}
	return ErrUserNotFound
}

func (s *Storage) ChangePassword(id, oldPassword, newPassword string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.ID == id {
			if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(oldPassword)); err != nil {
				return ErrWrongPassword
			}
			hashed, err := bcrypt.GenerateFromPassword([]byte(newPassword), bcrypt.DefaultCost)
			if err != nil {
				return err
			}
			user.PasswordHash = string(hashed)
			return s.save()
		}
	}
	return ErrUserNotFound
}

func (s *Storage) SetOnlineStatus(id string, online bool) {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.ID == id {
			user.IsOnline = online
			s.save()
			return
		}
	}
}

func (s *Storage) BanUser(id, reason, bannedBy string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.ID == id {
			user.IsBanned = true
			user.BanReason = reason
			return s.save()
		}
	}
	return ErrUserNotFound
}

func (s *Storage) UnbanUser(id string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.users {
		if user.ID == id {
			user.IsBanned = false
			user.BanReason = ""
			return s.save()
		}
	}
	return ErrUserNotFound
}

func (s *Storage) SearchUsers(query string) []*model.User {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var results []*model.User
	for _, u := range s.users {
		if contains(u.Username, query) || contains(u.Email, query) {
			results = append(results, u)
		}
	}
	return results
}

func (s *Storage) GetAllUsers() []*model.User {
	s.mu.RLock()
	defer s.mu.RUnlock()

	users := make([]*model.User, 0, len(s.users))
	for _, u := range s.users {
		users = append(users, u)
	}
	return users
}

func (s *Storage) GetOnlineCount() int {
	s.mu.RLock()
	defer s.mu.RUnlock()

	count := 0
	for _, u := range s.users {
		if u.IsOnline {
			count++
		}
	}
	return count
}

func (s *Storage) GetTotalCount() int {
	s.mu.RLock()
	defer s.mu.RUnlock()

	return len(s.users)
}

func contains(s, substr string) bool {
	return len(substr) == 0 || (len(s) >= len(substr) && containsIgnoreCase(s, substr))
}

func containsIgnoreCase(s, substr string) bool {
	s = toLower(s)
	substr = toLower(substr)
	for i := 0; i <= len(s)-len(substr); i++ {
		if s[i:i+len(substr)] == substr {
			return true
		}
	}
	return false
}

func toLower(s string) string {
	b := make([]byte, len(s))
	for i := 0; i < len(s); i++ {
		c := s[i]
		if c >= 'A' && c <= 'Z' {
			c += 'a' - 'A'
		}
		b[i] = c
	}
	return string(b)
}

func SetOnlineStatusGlobal(userID string, online bool) {
	defaultStorage := New("data/users.json")
	defaultStorage.SetOnlineStatus(userID, online)
}
