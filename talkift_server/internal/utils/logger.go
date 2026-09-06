package utils

import (
	"os"
	"path/filepath"

	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
)

var log *zap.SugaredLogger

func InitLogger(level string) {
	var lvl zapcore.Level
	if err := lvl.UnmarshalText([]byte(level)); err != nil {
		lvl = zapcore.InfoLevel
	}

	logDir := filepath.Join("data", "logs")
	os.MkdirAll(logDir, 0755)
	logFile := filepath.Join(logDir, "server.log")

	file, err := os.OpenFile(logFile, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		panic(err)
	}

	fileWriter := zapcore.AddSync(file)

	encoderConfig := zapcore.EncoderConfig{
		TimeKey:        "time",
		LevelKey:       "level",
		NameKey:        "logger",
		CallerKey:      "caller",
		MessageKey:     "msg",
		StacktraceKey:  "stacktrace",
		LineEnding:     zapcore.DefaultLineEnding,
		EncodeLevel:    zapcore.CapitalLevelEncoder,
		EncodeTime:     zapcore.ISO8601TimeEncoder,
		EncodeDuration: zapcore.SecondsDurationEncoder,
		EncodeCaller:   zapcore.ShortCallerEncoder,
	}

	core := zapcore.NewTee(
		zapcore.NewCore(
			zapcore.NewConsoleEncoder(encoderConfig),
			fileWriter,
			lvl,
		),
		zapcore.NewCore(
			zapcore.NewConsoleEncoder(encoderConfig),
			zapcore.AddSync(os.Stdout),
			lvl,
		),
	)

	logger := zap.New(core, zap.AddCallerSkip(1))
	log = logger.Sugar()
}

func Log() *zap.SugaredLogger {
	if log == nil {
		InitLogger("info")
	}
	return log
}

func Debugf(format string, args ...interface{}) { Log().Debugf(format, args...) }
func Infof(format string, args ...interface{})  { Log().Infof(format, args...) }
func Warnf(format string, args ...interface{})  { Log().Warnf(format, args...) }
func Errorf(format string, args ...interface{}) { Log().Errorf(format, args...) }
func Fatalf(format string, args ...interface{}) { Log().Fatalf(format, args...) }
