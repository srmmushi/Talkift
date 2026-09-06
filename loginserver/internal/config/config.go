package config

import (
	"os"
	"path/filepath"

	"github.com/BurntSushi/toml"
)

type Config struct {
	Server      ServerConfig      `toml:"server"`
	Storage     StorageConfig     `toml:"storage"`
	SSL         SSLConfig         `toml:"ssl"`
	Log         LogConfig         `toml:"log"`
}

type ServerConfig struct {
	Port int `toml:"port"`
}

type StorageConfig struct {
	Path string `toml:"path"`
}

type SSLConfig struct {
	Enable bool   `toml:"enable"`
	Cert   string `toml:"cert"`
	Key    string `toml:"key"`
}

type LogConfig struct {
	Path string `toml:"path"`
}

var defaultConfig = `# Talkift Login Server Configuration

[server]
port = 8081

[storage]
path = "data/users.json"

[ssl]
enable = false
cert = "certs/server.crt"
key = "certs/server.key"

[log]
path = "data/logs/access.log"
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
	if cfg.Storage.Path == "" {
		cfg.Storage.Path = "data/users.json"
	}
	if cfg.Log.Path == "" {
		cfg.Log.Path = "data/logs/access.log"
	}
	if cfg.SSL.Cert == "" {
		cfg.SSL.Cert = "certs/server.crt"
	}
	if cfg.SSL.Key == "" {
		cfg.SSL.Key = "certs/server.key"
	}
}

func EnsureDirs(cfg *Config) {
	dir := filepath.Dir(cfg.Storage.Path)
	if dir != "" {
		os.MkdirAll(dir, 0755)
	}

	logDir := filepath.Dir(cfg.Log.Path)
	if logDir != "" {
		os.MkdirAll(logDir, 0755)
	}
}
