package jwt

import (
	"fmt"
	"net/http"
	"time"
	"log"

	"github.com/dgrijalva/jwt-go"
	"github.com/spf13/viper"
)

var signData []byte

func Setup(config *viper.Viper) {
	signData = []byte(config.Get("jwt").(string))
}

// JWT schema of the data it will store
type Claims struct {
	Username string `json:"username"`
	Usertype string `json:"usertype"`
	jwt.StandardClaims
}

// create a JWT and put in the clients cookie
func SetToken(res http.ResponseWriter, req *http.Request, username string, usertype string) {
	expireToken := time.Now().Add(time.Hour * 12).Unix()
	expireCookie := time.Now().Add(time.Hour * 12)

	claims := Claims{
		username, usertype, 
		jwt.StandardClaims{
			ExpiresAt: expireToken,
			Issuer: "StarDisplayServer",
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	signedToken, _ := token.SignedString(signData)

	cookie := http.Cookie{Name: "Auth", Value: signedToken, Expires: expireCookie, HttpOnly: true}
	http.SetCookie(res, &cookie)
}

// middleware to protect private pages
func Validate(usertype string, redirectURI string, page http.HandlerFunc) http.HandlerFunc {
	return http.HandlerFunc(func(res http.ResponseWriter, req *http.Request) {
		//page(res, req)
		//return		

		cookie, err := req.Cookie("Auth")
		if err != nil {
			log.Print("no cookie")
			http.Redirect(res, req, redirectURI, 307)
			return
		}

		token, err := jwt.ParseWithClaims(cookie.Value, &Claims{}, func(token *jwt.Token) (interface{}, error) {
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				log.Print("bad sign")
				return nil, fmt.Errorf("Unexpected signing method")
			}
			return signData, nil
		})

		if err != nil || !token.Valid {
			log.Print("Invalid token")
			http.Redirect(res, req, redirectURI, 307)
			return
		}

		claims, ok := token.Claims.(*Claims)
		if !ok {
			log.Print("Invalid claims")
			http.Redirect(res, req, redirectURI, 307)
			return
		}

		if claims.Usertype != usertype {
			log.Print("Invalid usertype")
			http.Redirect(res, req, redirectURI, 307)
			return
		} 

		SetToken(res, req, claims.Username, claims.Usertype)
		page(res, req)
	})
}

// deletes the cookie
func Logout(res http.ResponseWriter, req *http.Request) {
	var deleteCookie = http.Cookie{Name: "Auth", Value: "none", Expires: time.Now()}
	http.SetCookie(res, &deleteCookie)
}

