package handlerssd

import (
	"net/http"
	"fmt"
	"strconv"

	"controllers/jwt"
)

func GetHome(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "Hello, dude")
}

func GetState(w http.ResponseWriter, r *http.Request) {
	jwt.SetToken(w, r, strconv.Itoa(100500), "user")
	longpollManager.SubscriptionHandler(w, r)
}

func PostState(w http.ResponseWriter, r *http.Request) {
	if err := r.ParseForm(); err != nil {
		fmt.Fprintf(w, "ParseForm() err: %v", err)
		return
	}
	
	for key, value := range r.PostForm {
		longpollManager.Publish(key, value[0])
	}

/*
	data := r.PostFormValue("stars")
	if data != "" {
		longpollManager.Publish("stars", data)
	}

	data = r.PostFormValue("overridestars")
	if data != "" {
		longpollManager.Publish("overridestars", data)
	}

	data = r.PostFormValue("net")
	if data != "" {
		longpollManager.Publish("net", data)
	}
*/
}
