package com.ubisam.example1;

import java.util.List;
import java.util.Optional;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;

@SpringBootTest
class Example1ApplicationTests {

	@Autowired
	private HelloRepository helloRepository;

	@Test
	void contextLoads() {
		Hello h = new Hello();
		h.setName("name1");
		h.setEmail("abc@ubisam.com");

		h = helloRepository.save(h);

        System.out.println("=== findByEmail ===");
        helloRepository.findByEmail("abc@ubisam.com")
            .forEach(x -> System.out.println("id=" + x.getId()));

        System.out.println("=== findByNameAndEmail ===");
        helloRepository.findByNameAndEmail("name1", "abc@ubisam.com")
            .forEach(x -> System.out.println("id=" + x.getId()));

        System.out.println("=== findByIdOrName ===");
        helloRepository.findByIdOrName(h.getId(), "name1")
            .forEach(x -> System.out.println("id=" + x.getId()));

		// Read
		Optional<Hello> h2 = helloRepository.findById(1l);
		System.out.println(h.getId());
		System.out.println(h2.get().getId());

		// Update
		h.setName("name2");
		h = helloRepository.save(h);

		// Search
		List<Hello> r = helloRepository.findAll();

		// Delete
		helloRepository.delete(h);
	}

}
