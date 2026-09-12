package config

import (
	"os"
	"path/filepath"

	"github.com/BurntSushi/toml"
)

type Config struct {
	Server     ServerConfig     `toml:"server"`
	ChatServer ChatServerConfig `toml:"chat_server"`
	Storage    StorageConfig    `toml:"storage"`
	SSL        SSLConfig        `toml:"ssl"`
	Log        LogConfig        `toml:"log"`
	Version    VersionConfig    `toml:"version"`
	RateLimit  RateLimitConfig  `toml:"rate_limit"`
	CORS       CORSConfig       `toml:"cors"`
}

type ServerConfig struct {
	Port      int    `toml:"port"`
	Name      string `toml:"name"`
	UniqueId  string `toml:"unique_id"`
	SecretKey string `toml:"secret_key"`
}

type ChatServerConfig struct {
	Host     string `toml:"host"`
	Port     int    `toml:"port"`
	Protocol string `toml:"protocol"`
}

type StorageConfig struct {
	Path        string `toml:"path"`
	VersionPath string `toml:"version_path"`
	ConfigPath  string `toml:"config_path"`
}

type SSLConfig struct {
	Enable bool   `toml:"enable"`
	Cert   string `toml:"cert"`
	Key    string `toml:"key"`
}

type LogConfig struct {
	Path  string `toml:"path"`
	Level string `toml:"level"`
}

type VersionConfig struct {
	Current   string `toml:"current"`
	MinClient string `toml:"min_client"`
	UpdateUrl string `toml:"update_url"`
}

type RateLimitConfig struct {
	Enabled       bool `toml:"enabled"`
	MaxRequests   int  `toml:"max_requests"`
	WindowSeconds int  `toml:"window_seconds"`
}

type CORSConfig struct {
	AllowedOrigins string `toml:"allowed_origins"`
	AllowedMethods string `toml:"allowed_methods"`
	AllowedHeaders string `toml:"allowed_headers"`
	MaxAge         int    `toml:"max_age"`
}

var defaultConfig = `# Talkift Official Server Configuration

[server]
port = 8081
name = "Talkift Official"
unique_id = "talkift-official-v1"
secret_key = "change-this-to-a-random-secret-key"

[chat_server]
host = "127.0.0.1"
port = 8080
protocol = "ws"

[storage]
path = "data/users.json"
version_path = "data/version"
config_path = "data/config"

[ssl]
enable = false
cert = "certs/server.crt"
key = "certs/server.key"

[log]
path = "data/logs/access.log"
level = "info"

[version]
current = "1.0.0"
min_client = "1.0.0"
update_url = "https://github.com/rmmushi/Talkift/releases"

[rate_limit]
enabled = true
max_requests = 100
window_seconds = 60

[cors]
allowed_origins = "*"
allowed_methods = "GET, POST, PUT, DELETE, OPTIONS"
allowed_headers = "Content-Type, Authorization"
max_age = 86400
`

func Load() (*Config, error) {
	configPath := "config.toml"

	if _, err := os.Stat(configPath); os.IsNotExist(err) {
		if err := os.WriteFile(configPath, []byte(defaultConfig), 0644); err != nil {
			return nil, err
		}
	}

	var cfg Config
	if _, err := toml.DecodeFile(configPath, &cfg); err != nil {
		return nil, err
	}

	applyDefaults(&cfg)
	return &cfg, nil
}

func applyDefaults(cfg *Config) {
	if cfg.Server.Port == 0 {
		cfg.Server.Port = 8081
	}
	if cfg.Server.Name == "" {
		cfg.Server.Name = "Talkift Official"
	}
	if cfg.Server.UniqueId == "" {
		cfg.Server.UniqueId = "talkift-official-v1"
	}
	if cfg.Server.SecretKey == "" {
		cfg.Server.SecretKey = "change-this-to-a-random-secret-key"
	}
	if cfg.ChatServer.Host == "" {
		cfg.ChatServer.Host = "127.0.0.1"
	}
	if cfg.ChatServer.Port == 0 {
		cfg.ChatServer.Port = 8080
	}
	if cfg.ChatServer.Protocol == "" {
		cfg.ChatServer.Protocol = "ws"
	}
	if cfg.Storage.Path == "" {
		cfg.Storage.Path = "data/users.json"
	}
	if cfg.Storage.VersionPath == "" {
		cfg.Storage.VersionPath = "data/version"
	}
	if cfg.Storage.ConfigPath == "" {
		cfg.Storage.ConfigPath = "data/config"
	}
	if cfg.Log.Path == "" {
		cfg.Log.Path = "data/logs/access.log"
	}
	if cfg.Log.Level == "" {
		cfg.Log.Level = "info"
	}
	if cfg.Version.Current == "" {
		cfg.Version.Current = "1.0.0"
	}
	if cfg.Version.MinClient == "" {
		cfg.Version.MinClient = "1.0.0"
	}
	if cfg.RateLimit.MaxRequests == 0 {
		cfg.RateLimit.MaxRequests = 100
	}
	if cfg.RateLimit.WindowSeconds == 0 {
		cfg.RateLimit.WindowSeconds = 60
	}
	if cfg.CORS.MaxAge == 0 {
		cfg.CORS.MaxAge = 86400
	}
}

func EnsureDirs(cfg *Config) {
	dirs := []string{
		filepath.Dir(cfg.Storage.Path),
		cfg.Storage.VersionPath,
		cfg.Storage.ConfigPath,
		filepath.Dir(cfg.Log.Path),
	}
	for _, dir := range dirs {
		if dir != "" {
			os.MkdirAll(dir, 0755)
		}
	}
}
