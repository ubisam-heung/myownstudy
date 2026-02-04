package com.ubisam.example1;

import java.util.List;

import org.springframework.web.bind.annotation.CrossOrigin;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/hello")
@CrossOrigin(origins = "http://localhost:5173")
public class HelloController {

    private final HelloRepository repo;

    public HelloController(HelloRepository repo) {
        this.repo = repo;
    }

    public record HelloRequest(String name, String email) {}

    @PostMapping
    public Hello create(@RequestBody HelloRequest req) {
        Hello h = new Hello();
        h.setName(req.name());
        h.setEmail(req.email());
        return repo.save(h);
    }

    @GetMapping
    public List<Hello> list() {
        return repo.findAll();
    }
}