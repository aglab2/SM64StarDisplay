package config

import (
    "github.com/spf13/viper"
)

func NewServerConfig() (*viper.Viper, error) {
    c := viper.New()
    c.SetDefault("http_addr", ":25565")
    c.SetDefault("http_cert_file", "")
    c.SetDefault("http_key_file", "")
    c.SetDefault("http_drain_interval", "1s")

    c.AutomaticEnv()

    return c, nil
}