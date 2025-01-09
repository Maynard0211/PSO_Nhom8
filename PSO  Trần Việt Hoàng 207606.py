import numpy as np
import matplotlib.pyplot as plt

# Thông số PSO
NUM_PARTICLES = 5       # Số lượng hạt
NUM_ITERATIONS = 50    # Số vòng lặp
NUM_TOWERS = 10           # Số lượng trạm phát sóng
AREA_WIDTH = 1000        # Chiều rộng khu vực
AREA_HEIGHT = 1000       # Chiều cao khu vực
COVERAGE_RADIUS = 150    # Bán kính phủ sóng
W_MAX = 0.9               # Trọng số quán tính tối đa
W_MIN = 0.4               # Trọng số quán tính tối thiểu
W = 0.5                  # Trọng số quán tính
C1 = 1.5                 # Trọng số kinh nghiệm cá nhân
C2 = 1.5                 # Trọng số kinh nghiệm toàn cục

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

# Khởi tạo các hạt
particles = [np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT], (NUM_TOWERS, 2)) for _ in range(NUM_PARTICLES)]
velocities = [np.random.uniform(-1, 1, (NUM_TOWERS, 2)) for _ in range(NUM_PARTICLES)]
personal_best = particles.copy()
personal_best_scores = [coverage_area(p) for p in particles]
global_best = personal_best[np.argmax(personal_best_scores)]
global_best_score = max(personal_best_scores)



# Vòng lặp PSO
for iteration in range(NUM_ITERATIONS):
    # Tính trọng số quán tính thay đổi theo vòng lặp
    W = W_MAX - (W_MAX - W_MIN) * (iteration / NUM_ITERATIONS)
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

    # In kết quả mỗi 10 vòng lặp
    if iteration % 10 == 0:
        print(f"Iteration {iteration}: Best Score = {global_best_score}")

        # Thêm chiến lược tái khởi tạo hạt nếu bị mắc kẹt trong cực trị cục bộ
    if iteration % 50 == 0:
        for i in range(NUM_PARTICLES):
            if np.all(velocities[i] == 0):  # Nếu tốc độ bằng 0, hạt có thể bị mắc kẹt
                particles[i] = np.random.uniform([0, 0], [AREA_WIDTH, AREA_HEIGHT], (NUM_TOWERS, 2))
                velocities[i] = np.random.uniform(-1, 1, (NUM_TOWERS, 2))

# Hiển thị kết quả
plt.figure(figsize=(10, 10))
for tower in global_best:
    circle = plt.Circle(tower, COVERAGE_RADIUS, color='blue', alpha=0.3)
    plt.gca().add_patch(circle)
plt.scatter(global_best[:, 0], global_best[:, 1], color='red', s=100, label='Cell Towers')
plt.scatter(points[:, 0], points[:, 1], color='green', s=1, alpha=0.2, label='Test Points')
plt.xlim(0, AREA_WIDTH)
plt.ylim(0, AREA_HEIGHT)
plt.xlabel('X Coordinate')
plt.ylabel('Y Coordinate')
plt.title('Optimized Cell Tower Placement using PSO')
plt.legend()
plt.grid()
plt.show()
