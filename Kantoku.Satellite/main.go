package main

import (
	"io"
	"log"
	"os"

	"gopkg.in/natefinch/npipe.v2"
)

func main() {
	pipe, err := npipe.Dial(`\\.\pipe\Kantoku`)
	if err != nil {
		log.Fatalf("failed to open pipe: %s", err)
	}

	go copy(os.Stdin, pipe)
	copy(pipe, os.Stdout)
}

func copy(r io.Reader, w io.Writer) {
	buf := make([]byte, 1024)

	for {
		read, err := r.Read(buf)
		if err != nil {
			break
		}

		w.Write(buf[:read])
	}
}
