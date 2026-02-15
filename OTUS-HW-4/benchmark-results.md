# Сравнение производительности динамических массивов

Дата замера: 2026-02-15 20:24:57
Параметры: InitialCount=2000, Iterations=1000

| Array | Operation | Order | Time (ms) |
|---|---|---:|---:|
| FactorArray | AddEnd | Ascending | 0,017 |
| VectorArray | AddEnd | Ascending | 0,025 |
| MatrixArray | AddEnd | Ascending | 0,030 |
| ArrayListWrapper | AddEnd | Ascending | 0,035 |
| SingleArray | AddEnd | Ascending | 1,542 |
| FactorArray | AddEnd | Descending | 0,016 |
| VectorArray | AddEnd | Descending | 0,020 |
| ArrayListWrapper | AddEnd | Descending | 0,024 |
| MatrixArray | AddEnd | Descending | 0,036 |
| SingleArray | AddEnd | Descending | 2,191 |
| FactorArray | AddEnd | Random | 0,028 |
| VectorArray | AddEnd | Random | 0,030 |
| ArrayListWrapper | AddEnd | Random | 0,038 |
| MatrixArray | AddEnd | Random | 0,054 |
| SingleArray | AddEnd | Random | 0,606 |
| ArrayListWrapper | InsertMiddle | Ascending | 0,241 |
| FactorArray | InsertMiddle | Ascending | 2,491 |
| VectorArray | InsertMiddle | Ascending | 2,547 |
| SingleArray | InsertMiddle | Ascending | 7,194 |
| MatrixArray | InsertMiddle | Ascending | 45,006 |
| ArrayListWrapper | InsertMiddle | Descending | 0,247 |
| FactorArray | InsertMiddle | Descending | 2,494 |
| VectorArray | InsertMiddle | Descending | 2,520 |
| SingleArray | InsertMiddle | Descending | 3,581 |
| MatrixArray | InsertMiddle | Descending | 44,929 |
| ArrayListWrapper | InsertMiddle | Random | 0,467 |
| FactorArray | InsertMiddle | Random | 2,541 |
| VectorArray | InsertMiddle | Random | 2,566 |
| SingleArray | InsertMiddle | Random | 3,401 |
| MatrixArray | InsertMiddle | Random | 44,917 |
| ArrayListWrapper | RemoveMiddle | Ascending | 0,273 |
| FactorArray | RemoveMiddle | Ascending | 2,846 |
| VectorArray | RemoveMiddle | Ascending | 2,846 |
| SingleArray | RemoveMiddle | Ascending | 4,266 |
| MatrixArray | RemoveMiddle | Ascending | 44,442 |
| ArrayListWrapper | RemoveMiddle | Descending | 0,268 |
| FactorArray | RemoveMiddle | Descending | 2,800 |
| VectorArray | RemoveMiddle | Descending | 2,827 |
| SingleArray | RemoveMiddle | Descending | 4,075 |
| MatrixArray | RemoveMiddle | Descending | 44,465 |
| ArrayListWrapper | RemoveMiddle | Random | 0,290 |
| FactorArray | RemoveMiddle | Random | 2,833 |
| VectorArray | RemoveMiddle | Random | 2,849 |
| SingleArray | RemoveMiddle | Random | 3,936 |
| MatrixArray | RemoveMiddle | Random | 44,848 |
| ArrayListWrapper | RemoveStart | Ascending | 0,395 |
| VectorArray | RemoveStart | Ascending | 5,569 |
| FactorArray | RemoveStart | Ascending | 5,588 |
| SingleArray | RemoveStart | Ascending | 6,942 |
| MatrixArray | RemoveStart | Ascending | 88,714 |
| ArrayListWrapper | RemoveStart | Descending | 0,412 |
| FactorArray | RemoveStart | Descending | 5,535 |
| VectorArray | RemoveStart | Descending | 5,579 |
| SingleArray | RemoveStart | Descending | 6,814 |
| MatrixArray | RemoveStart | Descending | 98,986 |
| ArrayListWrapper | RemoveStart | Random | 0,432 |
| VectorArray | RemoveStart | Random | 5,613 |
| FactorArray | RemoveStart | Random | 5,631 |
| SingleArray | RemoveStart | Random | 6,806 |
| MatrixArray | RemoveStart | Random | 88,791 |

## Выводы
1. `SingleArray` ожидаемо самый медленный на вставках/удалениях, потому что при каждом добавлении меняет размер на 1.
2. `VectorArray` и `FactorArray` стабильно быстрее `SingleArray` в операциях со сдвигом благодаря редкому перераспределению памяти.
3. В этой реализации `MatrixArray` медленнее на вставке/удалении по индексу, потому что сдвиг проходит через блочную адресацию на каждом шаге.
4. Порядок значений (`Ascending/Descending/Random`) почти не влияет на время, так как доминирует стоимость сдвига элементов и перераспределения памяти.
5. `ArrayListWrapper` показывает лучшие или близкие к лучшим результаты, так как использует оптимизированную внутреннюю реализацию платформы.
