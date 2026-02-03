package com.ubisam.example1;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import jakarta.transaction.Transactional;

@Component
public class HelloInsertExample {

    @Autowired
    private HelloRepository helloRepository;

    @Transactional
    public void insertHello() {
        Hello hello = new Hello();
        hello.setName("김길동");
        hello.setEmail("abc@naver.com");
        helloRepository.save(hello);
    }
}
