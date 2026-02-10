package com.ubisam.example2.Worlds;

import org.springframework.data.jpa.repository.JpaRepository;

public interface  WorldRepository extends JpaRepository<World, Long>{
    // List<World> findByName(String name);
    // List<World> findByContinentAndPopulation(String continent, Long population);
    // List<World> findByIdOrName(Long id, String name);
    
}
