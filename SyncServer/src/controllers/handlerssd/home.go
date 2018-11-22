package handlerssd

import (
	"net/http"
	"fmt"
	"log"
	"strconv"

	"controllers/jwt"
)

func GetHome(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "Hello, dude")
}

func GetState(w http.ResponseWriter, r *http.Request) {	
	log.Print("Longpoll was called")
	jwt.SetToken(w, r, strconv.Itoa(100500), "user")
	longpollManager.SubscriptionHandler(w, r)
}

func PostState(w http.ResponseWriter, r *http.Request) {
	r.ParseForm()

	log.Print("Post called")

	data := r.PostFormValue("stars")
	if data != "" {
		log.Print("It is stars!")
		longpollManager.Publish("stars", data)
	}

	data = r.PostFormValue("overridestars")
	if data != "" {
		log.Print("It is override!")
		longpollManager.Publish("overridestars", data)
	}
}
