package com.ubisam.example1;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;

public interface HelloRepository extends JpaRepository<Hello, Long> {
    List<Hello> findByEmail(String email);
}