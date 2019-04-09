package main

import (
	"net/http"
	"time"
    "io/ioutil"
	"fmt"
	"bufio"
	"os"
	"crypto/rand"
	"log"
	"strings"

	"github.com/tylerb/graceful"

	"conf/server"
	"controllers/application"
)

func GetOutboundIP() (string, error) {
	url := "https://api.ipify.org?format=text"
	resp, err := http.Get(url)
	if err != nil {
		return "", err
	}

	defer resp.Body.Close()
	ip, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}
	
	return string(ip), nil
}

func TryConnectToIP(ip string, port string) (error) {
	url := fmt.Sprintf("http://%s%s", ip, port)
	_, err := http.Get(url)
	return err
}

func GenerateRandomString(n int) (string) {
	const letters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-"
	bytes := make([]byte, n)
	rand.Read(bytes)
	for i, b := range bytes {
		bytes[i] = letters[b%byte(len(letters))]
	}
	return string(bytes)
}

func ServeByFile(path string, size int) (string, error) {
	data, err := ioutil.ReadFile(path)
	var str string
	if err != nil {
		str = GenerateRandomString(size)
		err = ioutil.WriteFile(path, []byte(str), 0500)
	} else {
		str = string(data[:])
	}
	return str, err
}

func main() {
	ip, err := GetOutboundIP()
	if err != nil {
		log.Print(err)
		return
	}

	config, err := config.NewServerConfig()
	if err != nil {
		fmt.Print(err)
		return
	}

	pwd, _ := os.Getwd()
	
	data, _ := ServeByFile(pwd + "/cookie", 32)
	config.Set("cookie_secret", data)

	data, _ = ServeByFile(pwd + "/jwt", 1024)
	config.Set("jwt", data)

	reader := bufio.NewReader(os.Stdin)
	fmt.Print("Enter Password\n")
	passwd, _ := reader.ReadString('\n')
	app, err := application.New(config, passwd)
	if err != nil {
		log.Print(err)
		return
	}
	
	fmt.Print("Enter Port\n")
	port, _ := reader.ReadString('\n')
	port = strings.TrimSpace(port)
	
	log.Print(fmt.Sprintf("Enter ip '%s' port '%s' in Star Display", ip, port))

	// TODO: This probably does not help with the fact that passwd is still on stack
	passwd = ""

	middle, err := app.MiddlewareStruct()
	if err != nil {
		log.Print(err)
		return
	}

	serverAddress := config.Get("http_addr").(string)
	if (port != "") {
		serverAddress = ":" + port
	}

	certFile := config.Get("http_cert_file").(string)
	keyFile := config.Get("http_key_file").(string)
	drainIntervalString := config.Get("http_drain_interval").(string)

	drainInterval, err := time.ParseDuration(drainIntervalString)
	if err != nil {
		log.Print(err)
		return
	}

	srv := &graceful.Server{
		Timeout: drainInterval,
		Server:  &http.Server{Addr: serverAddress, Handler: middle },
	}

	log.Print("Running HTTP server on " + serverAddress)
	
	go func(ip string, serverAddress string) {
		var conerr error
		for ok := true; ok; ok = conerr != nil {
			conerr = TryConnectToIP(ip, serverAddress)
			if conerr != nil {
				log.Print(fmt.Sprintf("Could not connect to itself, did you forward the port %s?", serverAddress[1:]))
				time.Sleep(4 * time.Second)
			} else {
				log.Print("Outbound connection works! Other players can connect to server")
			}
		}
	}(ip, serverAddress)

	if certFile != "" && keyFile != "" {
		err = srv.ListenAndServeTLS(certFile, keyFile)
	} else {
		err = srv.ListenAndServe()
	}

	if err != nil {
		log.Print(err)
	}
}
