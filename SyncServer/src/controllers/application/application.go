package application

import (
	"net/http"
    "strings"

	"github.com/carbocation/interpose"
	gorilla_mux "github.com/gorilla/mux"
	"github.com/gorilla/sessions"
	"github.com/spf13/viper"

	"controllers/handlerssd"
	"controllers/jwt"
)

// New is the constructor for Application struct.
func New(config *viper.Viper, passwd string) (*Application, error) {
	handlerssd.InitLongpoll()
	cookieStoreSecret := config.Get("cookie_secret").(string)

	jwt.Setup(config)
	app := &Application{}
	app.config = config
	app.sessionStore = sessions.NewCookieStore([]byte(cookieStoreSecret))
	handlerssd.SetPasswd(strings.TrimSpace(passwd))

	return app, nil
}

// Application is the application object that runs HTTP server.
type Application struct {
	config       *viper.Viper
	sessionStore sessions.Store
}

func (app *Application) MiddlewareStruct() (*interpose.Middleware, error) {
	middle := interpose.New()
	middle.UseHandler(app.mux())

	return middle, nil
}

func (app *Application) mux() *gorilla_mux.Router {
	router := gorilla_mux.NewRouter()

	router.Handle("/", http.HandlerFunc(handlerssd.GetHome)).Methods("GET")

	router.Handle("/sd/login", http.HandlerFunc(handlerssd.PostLogin)).Methods("POST")

	router.Handle("/sd/longpoll", http.HandlerFunc(
		jwt.Validate("user", "/",
			handlerssd.GetState))).Methods("GET")

	router.Handle("/sd/post", http.HandlerFunc(
		jwt.Validate("user", "/",
			handlerssd.PostState))).Methods("POST")

	// router.PathPrefix("/").Handler(http.FileServer(http.Dir("static")))

	return router
}
