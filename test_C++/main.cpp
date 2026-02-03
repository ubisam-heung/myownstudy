#include <iostream>
#include <string>
#include <sstream>
#include <algorithm>
#include <cctype>
#include <cstdlib>
using namespace std;

int read_first_int(const string& prompt) {
    const int MIN = 0;
    const int MAX = 150;
    const int MAX_TRIES = 3;

    for (int attempt = 1; attempt <= MAX_TRIES; attempt++) {
        cout << prompt << " (0~150, 종료: q) " << flush;

        string line;
        if (!getline(cin, line)) {
            // EOF 같은 상황
            return 0;
        }

        // 앞뒤 공백 제거
        auto not_space = [](unsigned char ch) { return !std::isspace(ch); };
        line.erase(line.begin(), std::find_if(line.begin(), line.end(), not_space));
        line.erase(std::find_if(line.rbegin(), line.rend(), not_space).base(), line.end());

        if (line == "q" || line == "Q") {
            cout << "입력을 종료합니다.\n";
            std::exit(0);
        }

        // 한 줄을 파싱해서 첫 번째 정수만 읽어본다 (예: "2 3" -> 2)
        stringstream ss(line);
        int x;
        if (ss >> x) {
            if (x >= MIN && x <= MAX) return x;
            cout << "범위를 벗어났습니다. " << MIN << "~" << MAX << " 사이로 입력하세요.\n";
        } else {
            cout << "숫자를 올바르게 입력하세요. 예: 23\n";
        }

        cout << "남은 시도: " << (MAX_TRIES - attempt) << "\n";
    }

    cout << "시도 횟수를 초과했습니다. 기본값 0을 사용합니다.\n";
    return 0;
}

int main() {
    ios::sync_with_stdio(false);
    cin.tie(nullptr);

    cout << "이름: " << flush;
    string name;
    getline(cin, name);

    int age = read_first_int("나이: ");

    cout << "안녕하세요 " << name << "님, 내년엔 " << (age + 1) << "살이에요.\n";
    return 0;
}