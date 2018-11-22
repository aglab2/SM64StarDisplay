package handlerssd

import (
	"net/http"
	"strconv"
	"fmt"
	"log"
	"golang.org/x/crypto/bcrypt"

	"controllers/jwt"
)

var hash []byte

func SetPasswd(passwd string) {
	hash, _ = bcrypt.GenerateFromPassword([]byte(passwd), bcrypt.MinCost)
}

func PostLogin(w http.ResponseWriter, r *http.Request) {
	r.ParseForm()

	password := r.PostFormValue("password")
	if bcrypt.CompareHashAndPassword(hash, []byte(password)) == nil {
		jwt.SetToken(w, r, strconv.Itoa(100500), "user")
	}else{
		log.Print(fmt.Sprintf("Attempt to use passwd '%s'", password))
		w.WriteHeader(http.StatusForbidden)
	}
}
