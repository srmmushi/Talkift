package config

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
	"strings"

	"github.com/BurntSushi/toml"
)

type ServerConfig struct {
	Port       int    `toml:"port"`
	SSLEnabled bool   `toml:"ssl_enabled"`
	SSLCert    string `toml:"ssl_cert"`
	SSLKey     string `toml:"ssl_key"`
}

type StorageConfig struct {
	Type           string `toml:"type"`
	RetainMessages bool   `toml:"retain_messages"`
}

type LoginServerConfig struct {
	Official          bool     `toml:"official"`
	Offline           bool     `toml:"offline"`
	ThirdPartyEnabled bool     `toml:"third_party_enabled"`
	ThirdPartyServers []string `toml:"third_party_servers"`
}

type LoggingConfig struct {
	Level string `toml:"level"`
}

type Config struct {
	Server      ServerConfig      `toml:"server"`
	Storage     StorageConfig     `toml:"storage"`
	LoginServer LoginServerConfig `toml:"loginserver"`
	Logging     LoggingConfig     `toml:"logging"`
}

func DefaultConfig() *Config {
	return &Config{
		Server: ServerConfig{
			Port:       8080,
			SSLEnabled: false,
			SSLCert:    "",
			SSLKey:     "",
		},
		Storage: StorageConfig{
			Type:           "local",
			RetainMessages: true,
		},
		LoginServer: LoginServerConfig{
			Official:          true,
			Offline:           false,
			ThirdPartyEnabled: false,
			ThirdPartyServers: []string{},
		},
		Logging: LoggingConfig{
			Level: "info",
		},
	}
}

func Load(path string) (*Config, error) {
	if _, err := os.Stat(path); os.IsNotExist(err) {
		fmt.Println("config.toml not found, starting configuration wizard...")
		cfg, err := runWizard()
		if err != nil {
			return nil, fmt.Errorf("configuration wizard failed: %w", err)
		}
		if err := Save(path, cfg); err != nil {
			return nil, fmt.Errorf("failed to save config: %w", err)
		}
		return cfg, nil
	}

	var cfg Config
	if _, err := toml.DecodeFile(path, &cfg); err != nil {
		return nil, err
	}
	return &cfg, nil
}

func Save(path string, cfg *Config) error {
	f, err := os.Create(path)
	if err != nil {
		return err
	}
	defer f.Close()

	enc := toml.NewEncoder(f)
	return enc.Encode(cfg)
}

func runWizard() (*Config, error) {
	reader := bufio.NewReader(os.Stdin)
	cfg := DefaultConfig()

	fmt.Println("\n=== Talkift Server Configuration Wizard ===")

	fmt.Printf("Server port [%d]: ", cfg.Server.Port)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(input)
		if input != "" {
			if port, err := strconv.Atoi(input); err == nil {
				cfg.Server.Port = port
			}
		}
	}

	fmt.Printf("Enable SSL (y/N) [%t]: ", cfg.Server.SSLEnabled)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(strings.ToLower(input))
		if input == "y" || input == "yes" {
			cfg.Server.SSLEnabled = true
			fmt.Print("SSL certificate path: ")
			cert, _ := reader.ReadString('\n')
			cfg.Server.SSLCert = strings.TrimSpace(cert)
			fmt.Print("SSL key path: ")
			key, _ := reader.ReadString('\n')
			cfg.Server.SSLKey = strings.TrimSpace(key)
		}
	}

	fmt.Printf("Storage type (local) [%s]: ", cfg.Storage.Type)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(input)
		if input != "" {
			cfg.Storage.Type = input
		}
	}

	fmt.Printf("Retain messages (Y/n) [%t]: ", cfg.Storage.RetainMessages)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(strings.ToLower(input))
		if input == "n" || input == "no" {
			cfg.Storage.RetainMessages = false
		}
	}

	fmt.Printf("Enable third-party registration (y/N) [%t]: ", cfg.LoginServer.ThirdPartyEnabled)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(strings.ToLower(input))
		if input == "y" || input == "yes" {
			cfg.LoginServer.ThirdPartyEnabled = true
			fmt.Print("Third-party server addresses (comma-separated): ")
			servers, _ := reader.ReadString('\n')
			for _, s := range strings.Split(strings.TrimSpace(servers), ",") {
				s = strings.TrimSpace(s)
				if s != "" {
					cfg.LoginServer.ThirdPartyServers = append(cfg.LoginServer.ThirdPartyServers, s)
				}
			}
		}
	}

	fmt.Printf("Log level (debug/info/warn/error) [%s]: ", cfg.Logging.Level)
	if input, err := reader.ReadString('\n'); err == nil {
		input = strings.TrimSpace(input)
		if input != "" {
			cfg.Logging.Level = input
		}
	}

	fmt.Println("\nConfiguration saved.")
	return cfg, nil
}
