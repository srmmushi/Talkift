package storage

import (
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"strings"
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
	mu        sync.RWMutex
	usersDir  string
	users     map[string]*model.User   // key: username
	idIndex   map[string]string        // key: user_id -> username
	emailIndex map[string]string       // key: email -> username
}

func New(usersDir string) *Storage {
	os.MkdirAll(usersDir, 0755)
	s := &Storage{
		usersDir:   usersDir,
		users:      make(map[string]*model.User),
		idIndex:    make(map[string]string),
		emailIndex: make(map[string]string),
	}
	s.loadAll()
	return s
}

func (s *Storage) userPath(token string) string {
	return filepath.Join(s.usersDir, token+".json")
}

func (s *Storage) loadAll() {
	entries, err := os.ReadDir(s.usersDir)
	if err != nil {
		return
	}

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".json") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(s.usersDir, entry.Name()))
		if err != nil {
			continue
		}

		var user model.User
		if err := json.Unmarshal(data, &user); err != nil {
			continue
		}

		s.users[user.Username] = &user
		s.idIndex[user.ID] = user.Username
		if user.Email != "" {
			s.emailIndex[user.Email] = user.Username
		}
	}
}

func (s *Storage) saveUser(user *model.User) error {
	data, err := json.MarshalIndent(user, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(s.userPath(user.ID), data, 0644)
}

func (s *Storage) CreateUser(username, password, email, registerMethod string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	if _, exists := s.users[username]; exists {
		return nil, ErrUsernameExists
	}

	if email != "" {
		if _, exists := s.emailIndex[email]; exists {
			return nil, ErrEmailExists
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

	if err := s.saveUser(user); err != nil {
		return nil, err
	}

	s.users[username] = user
	s.idIndex[user.ID] = username
	if email != "" {
		s.emailIndex[email] = username
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
	s.saveUser(user)

	return user, nil
}

func (s *Storage) AuthenticateByEmail(email, password string) (*model.User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.emailIndex[email]
	if !exists {
		return nil, ErrUserNotFound
	}

	user := s.users[username]

	if user.IsBanned {
		return nil, ErrUserBanned
	}

	if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(password)); err != nil {
		return nil, ErrWrongPassword
	}

	user.LastLoginAt = time.Now()
	s.saveUser(user)

	return user, nil
}

func (s *Storage) GetUserByUsername(username string) (*model.User, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	user, exists := s.users[username]
	return user, exists
}

func (s *Storage) GetUserByID(id string) (*model.User, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()
	username, exists := s.idIndex[id]
	if !exists {
		return nil, false
	}
	return s.users[username], true
}

func (s *Storage) UpdateUser(id string, updates map[string]interface{}) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.idIndex[id]
	if !exists {
		return ErrUserNotFound
	}

	user := s.users[username]

	if v, ok := updates["email"].(string); ok {
		if user.Email != v {
			if user.Email != "" {
				delete(s.emailIndex, user.Email)
			}
			user.Email = v
			if v != "" {
				s.emailIndex[v] = username
			}
		}
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

	return s.saveUser(user)
}

func (s *Storage) ChangePassword(id, oldPassword, newPassword string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.idIndex[id]
	if !exists {
		return ErrUserNotFound
	}

	user := s.users[username]

	if err := bcrypt.CompareHashAndPassword([]byte(user.PasswordHash), []byte(oldPassword)); err != nil {
		return ErrWrongPassword
	}

	hashed, err := bcrypt.GenerateFromPassword([]byte(newPassword), bcrypt.DefaultCost)
	if err != nil {
		return err
	}

	user.PasswordHash = string(hashed)
	return s.saveUser(user)
}

func (s *Storage) SetOnlineStatus(id string, online bool) {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.idIndex[id]
	if !exists {
		return
	}

	s.users[username].IsOnline = online
	s.saveUser(s.users[username])
}

func (s *Storage) BanUser(id, reason, bannedBy string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.idIndex[id]
	if !exists {
		return ErrUserNotFound
	}

	user := s.users[username]
	user.IsBanned = true
	user.BanReason = reason
	return s.saveUser(user)
}

func (s *Storage) UnbanUser(id string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	username, exists := s.idIndex[id]
	if !exists {
		return ErrUserNotFound
	}

	user := s.users[username]
	user.IsBanned = false
	user.BanReason = ""
	return s.saveUser(user)
}

func (s *Storage) SearchUsers(query string) []*model.User {
	s.mu.RLock()
	defer s.mu.RUnlock()

	query = strings.ToLower(query)
	var results []*model.User
	for _, u := range s.users {
		if strings.Contains(strings.ToLower(u.Username), query) ||
			strings.Contains(strings.ToLower(u.Email), query) {
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

func (s *Storage) GetToken(user *model.User) string {
	return fmt.Sprintf("%s|%d", user.ID, user.CreatedAt.Unix())
}
