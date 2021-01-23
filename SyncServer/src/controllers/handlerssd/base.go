// Package handlers provides request handlers.
package handlerssd

import (
	"log"

	"github.com/jcuga/golongpoll"
)

var longpollManager *golongpoll.LongpollManager

func InitLongpoll() {
	var err error
	longpollManager, err = golongpoll.StartLongpoll(golongpoll.Options{
		LoggingEnabled: false,
		EventTimeToLiveSeconds: 60,
	})

	if err != nil {
		log.Fatal(err)
	}
}