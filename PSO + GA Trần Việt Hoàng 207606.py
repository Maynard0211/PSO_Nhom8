import numpy as np
import matplotlib.pyplot as plt

# Thông số PSO và GA
NUM_PARTICLES = 30  # Số lượng hạt PSO
NUM_ITERATIONS = 100  # Số vòng lặp
NUM_TOWERS = 10  # Số lượng trạm phát sóng
AREA_WIDTH = 1000  # Chiều rộng khu vực
AREA_HEIGHT = 1000  # Chiều cao khu vực
COVERAGE_RADIUS = 150  # Bán kính phủ sóng
W_MAX = 0.9  # Trọng số quán tính tối đa
W_MIN = 0.4  # Trọng số quán tính tối thiểu
C1 = 1.5  # Trọng số kinh nghiệm cá nhân
C2 = 1.5  # Trọng số kinh nghiệm toàn cục
MUTATION_RATE = 0.1  # Tỉ lệ đột biến của GA
CROSSOVER_RATE = 0.8  # Tỉ lệ giao phối của GA
NUM_GENERATIONS = 50  # Số thế hệ của GA

# Tạo các điểm kiểm tra và trạm phát ngẫu nhiên trong khu vực
NUM_POINTS = 10000
points = np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT], (NUM_POINTS, 2))
towers = np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT], (NUM_TOWERS, 2))


# Hàm tính vùng phủ sóng
def coverage_area(towers):
    covered_points = 0
    for point in points:
        for tower in towers:
            distance = np.linalg.norm(point - tower)
            if distance <= COVERAGE_RADIUS:
                covered_points += 1
                break
    return covered_points


# Khởi tạo các hạt PSO
particles = [np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT], (NUM_TOWERS, 2)) for _ in range(NUM_PARTICLES)]
velocities = [np.random.uniform(-1, 1, (NUM_TOWERS, 2)) for _ in range(NUM_PARTICLES)]
personal_best = particles.copy()
personal_best_scores = [coverage_area(p) for p in particles]
global_best = personal_best[np.argmax(personal_best_scores)]
global_best_score = max(personal_best_scores)


# Hàm crossover (Giao phối)
def crossover(parent1, parent2):
    crossover_point = np.random.randint(1, NUM_TOWERS)  # Chọn điểm cắt ngẫu nhiên
    child1 = np.concatenate((parent1[:crossover_point], parent2[crossover_point:]))
    child2 = np.concatenate((parent2[:crossover_point], parent1[crossover_point:]))
    return child1, child2


# Hàm mutation (Đột biến)
def mutate(child):
    if np.random.rand() < MUTATION_RATE:
        mutation_point = np.random.randint(0, NUM_TOWERS)
        child[mutation_point] = np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT])
    return child


# Vòng lặp PSO và GA kết hợp
for iteration in range(NUM_ITERATIONS):
    # Tính trọng số quán tính thay đổi theo vòng lặp
    W = W_MAX * (W_MIN / W_MAX) ** (iteration / NUM_ITERATIONS)

    for i in range(NUM_PARTICLES):
        # Tính hàm mục tiêu
        current_score = coverage_area(particles[i])

        # Cập nhật personal best
        if current_score > personal_best_scores[i]:
            personal_best[i] = particles[i].copy()
            personal_best_scores[i] = current_score

        # Cập nhật global best
        if current_score > global_best_score:
            global_best = particles[i].copy()
            global_best_score = current_score

        # Cập nhật tốc độ và vị trí
        r1 = np.random.random((NUM_TOWERS, 2))
        r2 = np.random.random((NUM_TOWERS, 2))

        velocities[i] = (W * velocities[i]
                         + C1 * r1 * (personal_best[i] - particles[i])
                         + C2 * r2 * (global_best - particles[i]))
        particles[i] += velocities[i]

        # Giới hạn vị trí trong khu vực
        particles[i] = np.clip(particles[i], [0, 0], [AREA_WIDTH, AREA_HEIGHT])

    # **Kết hợp GA sau mỗi vòng lặp PSO**
    if iteration % 10 == 0:
        # Tạo thế hệ mới từ các hạt PSO
        new_particles = []
        for i in range(0, NUM_PARTICLES, 2):
            parent1 = particles[i]
            parent2 = particles[i + 1] if i + 1 < NUM_PARTICLES else particles[i]

            # Crossover giữa hai hạt
            if np.random.rand() < CROSSOVER_RATE:
                child1, child2 = crossover(parent1, parent2)
            else:
                child1, child2 = parent1.copy(), parent2.copy()

            # Đột biến các hạt
            child1 = mutate(child1)
            child2 = mutate(child2)

            # Thêm các hạt con vào quần thể
            new_particles.append(child1)
            new_particles.append(child2)

        particles = new_particles  # Cập nhật lại quần thể hạt

    # In kết quả mỗi 10 vòng lặp
    if iteration % 10 == 0:
        print(f"Iteration {iteration}: Best Score = {global_best_score}")

# Hiển thị kết quả
plt.figure(figsize=(10, 10))
for tower in global_best:
    circle = plt.Circle(tower, COVERAGE_RADIUS, color='blue', alpha=0.3)
    plt.gca().add_patch(circle)

# Hiển thị tất cả các trạm phát sóng và điểm kiểm tra
plt.scatter(global_best[:, 0], global_best[:, 1], color='red', s=100, label='Cell Towers')
plt.scatter(points[:, 0], points[:, 1], color='green', s=1, alpha=0.2, label='Test Points')
plt.xlim(0, AREA_WIDTH)
plt.ylim(0, AREA_HEIGHT)
plt.xlabel('X Coordinate')
plt.ylabel('Y Coordinate')
plt.title('Optimized Cell Tower Placement using PSO and GA')
plt.legend()
plt.grid()
plt.show()
