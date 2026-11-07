namespace Model.classes
{
    public class BaseEntity
    {
        private int id;

        public BaseEntity(int id)
        {
            this.id = id;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }
    }
}
