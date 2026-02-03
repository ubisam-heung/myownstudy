package com.ubisam.example1;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.stereotype.Component;

import jakarta.transaction.Transactional;

@Component
public class HelloInsertExample implements ApplicationRunner {

    private static final Logger log = LoggerFactory.getLogger(HelloInsertExample.class);

    @Autowired
    private HelloRepository helloRepository;

    @Transactional
    public void insertOrUpdateHello() {
        String name = "김길동";
        String email = "abc@naver.com";

        List<Hello> existing = helloRepository.findByEmail(email);
        Hello hello;
        if (existing != null && !existing.isEmpty()) {
            hello = existing.get(0);
            hello.setName(name); // overwrite existing data
            hello.setEmail(email);
            log.info("Updating existing Hello with email={}", email);
        } else {
            hello = new Hello();
            hello.setName(name);
            hello.setEmail(email);
            log.info("Inserting new Hello with email={}", email);
        }
        helloRepository.save(hello);
    }

    @Override
    public void run(ApplicationArguments args) throws Exception {
        log.info("HelloInsertExample running at startup");
        try {
            insertOrUpdateHello();
        } catch (Exception e) {
            // don't fail application startup if DB is unavailable; log and continue
            log.error("Failed to insert/update Hello at startup: {}", e.getMessage(), e);
        }
    }
}
