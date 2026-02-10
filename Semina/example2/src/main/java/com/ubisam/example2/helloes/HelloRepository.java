package com.ubisam.example2.helloes;

import org.springframework.data.jpa.repository.JpaRepository;


public interface HelloRepository extends JpaRepository<Hello, Long>{
    // List<Hello> findByEmail(String email);
    // List<Hello> findByNameAndEmail(String name, String email);
    // List<Hello> findByIdOrName(Long id, String name);
}
