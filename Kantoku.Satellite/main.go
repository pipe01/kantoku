package main

import (
	"io"
	"log"
	"os"

	"gopkg.in/natefinch/npipe.v2"
)

func main() {
	var pipeWriter io.Writer
	go copy(os.Stdin, &pipeWriter)

	pipe, err := npipe.Dial(`\\.\pipe\Kantoku`)
	if err != nil {
		log.Fatalf("failed to open pipe: %s", err)
	}

	pipeWriter = pipe

	var stdout io.Writer = os.Stdout
	copy(pipe, &stdout)
}

func copy(r io.Reader, w *io.Writer) {
	buf := make([]byte, 1024)

	for {
		read, err := r.Read(buf)
		if err != nil {
			break
		}

		if *w != nil {
			(*w).Write(buf[:read])
		}
	}
}
