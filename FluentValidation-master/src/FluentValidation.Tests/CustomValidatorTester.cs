#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at https://github.com/jeremyskinner/FluentValidation
#endregion

namespace FluentValidation.Tests {
	using System.Linq;
	using System.Threading.Tasks;
	using Xunit;
	using Results;

	
	public class CustomValidatorTester {
		private TestValidator validator;
		public CustomValidatorTester() {
			validator = new TestValidator();
		}

		[Fact]
		public void Returns_single_failure() {
			validator.Custom(person => new ValidationFailure("Surname", "Fail", null));
			var result = validator.Validate(new Person());

			result.IsValid.ShouldBeFalse();
			result.Errors[0].ErrorMessage.ShouldEqual("Fail");
			result.Errors[0].PropertyName.ShouldEqual("Surname");
		}

		public void Returns_single_failure_async() {
			validator.CustomAsync(async person => new ValidationFailure("Surname", "Fail", null));
			var result = validator.ValidateAsync(new Person()).Result;

			result.IsValid.ShouldBeFalse();
			result.Errors[0].ErrorMessage.ShouldEqual("Fail");
			result.Errors[0].PropertyName.ShouldEqual("Surname");
		}

		[Fact]
		public void When_the_lambda_returns_null_then_the_validation_should_succeed() {
			validator.Custom(person => null);
			var result = validator.Validate(new Person());

			result.IsValid.ShouldBeTrue();
		}

		[Fact]
		public void When_the_async_lambda_returns_null_then_the_validation_should_succeed()
		{
			validator.CustomAsync(async person => null);
			var result = validator.ValidateAsync(new Person()).Result;

			result.IsValid.ShouldBeTrue();
		}

		[Fact]
		public void Perserves_property_chain_using_custom() {
			validator.RuleFor(x => x.Orders).SetCollectionValidator(new NestedOrderValidator());
			var person = new Person();
			person.Orders.Add(new Order());
			var result = validator.Validate(person);

			result.Errors.Single().PropertyName.ShouldEqual("Orders[0].Amount");
		}

		[Fact]
		public void Custom_within_ruleset() {
			var validator = new InlineValidator<Person>();
			validator.RuleSet("foo", () => { validator.Custom(x => { return new ValidationFailure("x", "y"); }); });
			validator.RuleSet("bar", () => { validator.Custom(x => { return new ValidationFailure("x", "y"); }); });

			var result = validator.Validate(new Person(), ruleSet: "foo");
			result.Errors.Count.ShouldEqual(1);
		}

		[Fact]
		public void CustomAsync_within_ruleset()
		{
			var validator = new InlineValidator<Person>();
			validator.RuleSet("foo", () => validator.CustomAsync(async x => new ValidationFailure("x", "y")));
			validator.RuleSet("bar", () => validator.CustomAsync(async x =>new ValidationFailure("x", "y")));

			var result = validator.ValidateAsync(new Person(), ruleSet: "foo").Result;
			result.Errors.Count.ShouldEqual(1);
		}

		private class NestedOrderValidator : AbstractValidator<Order> {
			public NestedOrderValidator() {
				Custom((x, ctx) => {
					return new ValidationFailure(ctx.PropertyChain.BuildPropertyName("Amount"), "bar");
				});
			}
		}

	}
}