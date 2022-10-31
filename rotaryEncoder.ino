#include <Uduino.h>
#define  A_PHASE 2
#define  B_PHASE 3
int val= 0;

Uduino uduino("leftRotary"); //change the name and televerse to filter them in Unity

void setup() {
  pinMode(A_PHASE, INPUT);
  pinMode(B_PHASE, INPUT);
  Serial.begin(9600);
  Serial.println("Connected");
  attachInterrupt(digitalPinToInterrupt( A_PHASE), interrupt, RISING); //Interrupt trigger mode: RISING
}
void loop() {
  uduino.update();
  if(uduino.isConnected()){
    Serial.println(-val);
    delay(100);
  }
  

}
void interrupt()// Interrupt function
{ char i;
  i = digitalRead( B_PHASE);
  if (i == 1)
    val += 1;
  else
    val -= 1;
}
