
<h1>For CarX</h1>
<h2>Архитектурные решения</h2>


+ Добавлен класс `Projectile`, родитель для всех классов снарядов.
+ Добавлен класс `Tower`, родитель для всех классов башен. Также в нем реализован пул объектов (`Projectile`).
+ `CannonProjectile` относится к `CannonTower`, что ясно из имени класса. Однако это нельзя сказать про `GuidedProjectile` и `SimpleTower`. Для интуитивного понимания переименовал их в `MagicProjectile` и `MagicTower` соответственно.
+ Вместо поиска всех монстров на сцене, добавил для башен триггер. При входе или выходе монстра из триггера он добавляется или удаляется из списка доступных целей в `Tower`.

<h2>Найденные недочеты</h2>

<h3><i>CannonProjectile.cs</i></h3>

1. Модификатор должен быть `private` с аттрибутом `[SerializeField]` (данный недочет встречается и в других местах проекта. Исправлено во всех местах, далее в тексте к этому недочету возвращаться не буду).
   <br>До:
   ```c#
   public float m_speed = 0.2f;
   ```
   После:
   ```c#
   [SerializeField] private float m_speed = 0.2f;
   ```
2. Обращение к `transfrom` каждый кадр, необходимо кэшировать (данный недочет встречается и в других местах проекта. Исправлено во всех местах, далее в тексте к этому недочету возвращаться не буду).
   <br>До:
   ```c#
   private void Update () {
		var translation = transform.forward * m_speed;
		transform.Translate (translation);
	}
   ```
   После:
   ```c#
    private Transform m_transform;
	private void Awake() {
		m_transform = GetComponent<Transform>();
	}
	private void Update () {
		var translation = _transform.forward * m_speed;
		m_transform.Translate (translation);
	}
   ```
3. В соответствии с принципом SOLID (прицип единой ответственности) при контроле поведения `Monster` необходимо вынести понижение HP и проверку `(monster.m_hp <= 0)` в класс `Monster`.
   <br>До:
   <br><i>CannonProjectile.cs</i>
   ```c#
   monster.m_hp -= m_damage;
   if (monster.m_hp <= 0) {
      Destroy (monster.gameObject);
   }
   ```
   <br>После:
   <br><i>CannonProjectile.cs</i>
   ```c#
   monster.TakeDamage(m_damage);
   ```
   <i>Monster.cs</i>
   ```c#
   public void TakeDamage(int damage) {
		m_hp -= damage;
		if (m_hp <= 0) {
			Destroy(gameObject);
		}
	}
   ```
4. В соответствии с общепринятыми соглашениями по написанию кода были замечены такие недочеты, как разное количество переносов строк между определениями методов, пробелы в местах вызова методов и между названием метода и открывающей скобкой. Данные недочеты были исправлены во всем проекте, далее в тексте к ним возвращаться не буду.

<h3><i>CannonTower.cs</i></h3>

1. Проверку на наличие префаба достаточно выполнять один раз.


   ```c#
	void Update () {
		if (m_projectilePrefab == null || m_shootPoint == null)
			return;
   ```
 
   ```c#
   private void Start() {
		if (m_projectilePrefab == null)
			Destroy(gameObject);
	}

	private void Update () {
		if (m_shootPoint == null)
			return;
   ```
   
2. Для экономии ресурсов при поиске всех объектов на сцене с компонентом `Monster` необходимо использовать `SphereCollider`.
3. Изначально в проекте проверка осуществлялась в каждой итерации цила `if (m_lastShotTime + m_shootInterval > Time.time)`, однако в связи с тем, что изменение `Time.time` в пределах вызова метода `Update()` незначительное, достаточно осуществить проверку один раз в самом методе `Update()`.

4. Вместо обычного создания объекта использовать <b>пул объектов</b>.
   ```c#
      private void InitPoolProjectiles() {
         for (int i = 0; i < m_poolSize; i++)
         {
               var obj = Instantiate(m_projectilePrefab);
               obj.Deactivate();
               m_objectPool.Add(obj);
         }
      }

      protected Projectile RunProjectile(Vector3 position, Quaternion rotation) {
         for (int i = 0; i < m_objectPool.Count; i++)
         {
               if (!m_objectPool[i].IsRunned)
               {
                  m_objectPool[i].Run(position,rotation);
                  return m_objectPool[i];
               }
         }
         return null;
      }
   ```

<br>
<h3><i>GuidedProjectile.cs</i></h3>


1. Выражение избыточное:
   ```c#
   var monster = other.gameObject.GetComponent<Monster>();
   ```
   Было измененено на следующее:
   ```c#
   var monster = other.GetComponent<Monster>();
   ```
2. Контроль поведения `Monster`, был изменен аналогично <i>CannonProjectile.cs</i>.
<br>
<h3><i>Monster.cs</i></h3>

1. До изменений в проекте высчитывалась длина, однако для экономии ресурсов необходимо считать ее квадрат:
   ```c#
   	if (Vector3.Distance(transform.position, m_moveTarget.transform.position) <= m_reachDistance)
   ```
   
   ```c#
      if (Vector3.SqrMagnitude(transform.position - m_moveTarget.transform.position) <= m_reachDistance)
   ```



<h3><i>SimpleTower.cs</i></h3>

1. Проверка наличия префаба была изменена аналогично <i>CannonTower.cs</i>.
2. Поиск объектов был изменен аналогично <i>CannonTower.cs</i>.
3. Выражение избыточное:
   ```c#
   var projectile = Instantiate(m_projectilePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity) as GameObject;
   ```
   Было изменено на следующее:
   ```c#
   var projectile = Instantiate(m_projectilePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
   ```
4. В соответствии с принципами наименования переменных в проекте было изменено:
   ```c#
   var projectileBeh = projectile.GetComponent<GuidedProjectile> ();
   ```
   ```c#
   var projectile = projectile.GetComponent<GuidedProjectile>();
   ```
5. В проекте изменялось поведение `GuidedProjectile` прямо в классе `SimpleTower`:
   ```c#
   projectileBeh.m_target = monster.gameObject;
   ```
   В соответствии с принципом SOLID был использован публичный метод:
   ```c#
   projectileBeh.SetTarget(monster.gameObject);
   ```

<h3><i>Spawner.cs</i></h3>

1. В большей части проекта проверка времени в таком виде:
   ```c#
      if (m_lastShotTime + m_shootInterval > Time.time)
            return;
   ```

   Однако в данном классе она осуществлялась другим образом:
   ```c#
      if (Time.time > m_lastSpawn + m_interval) {
            ...
         }
   ```
   Было заменено на единый формат.
2. В проекте осуществлялось изменение поведения `Monster` прямо в классе `Spawner`:
   ```c#
   monsterBeh.m_moveTarget = m_moveTarget;
   ```
   В соответствии с принципом SOLID был использован публичный метод.
   ```c#
   monsterBeh.SetTarget(m_moveTarget);
   ```

<h2>Премечание</h2>

Для <b>параболической траектории</b> снаряда необходимо изменить значение поля `CannonTowerType` в компоненте <i>GuidedProjectile</i> на `Physics`, Assets/Prefabs/CannonProjectilePhysics.prefab использовать в качестве `ProjectilePrefab`  